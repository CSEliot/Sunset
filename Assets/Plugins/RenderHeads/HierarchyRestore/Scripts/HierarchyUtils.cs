#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0)
	#define RH_UNITY_FEATURE_WINDOWTITLE
	#define RH_UNITY_FEATURE_DEBUG_ASSERT
#endif
using UnityEngine;
// Note: This script is only for the editor, but we don't put it into an Editor folder because it needs to be attached to a Monobehaviour
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2017 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Framework.Editor
{
	/// <summary>
	/// Utility functions for getting and setting the expanded state of the Unity hierarchy window
	/// Also has functions for getting and setting the hierarchy window selection
	/// NOTE: there are no functions to return which scene nodes are expanded
	/// </summary>
	public static class HierarchyUtils
	{
		public static string GetHierarchyAsString()
		{
			string result = string.Empty;
			// TODO: Optimise this - it must be quite slow to always use reflection, so need to cache these values
			GameObject[] gos = GetHierarchyWindowExpandedObjects();

			// Convert transforms into a path list separated by '*' character
			if (gos != null && gos.Length > 0)
			{
				foreach (GameObject go in gos)
				{
					string path = AnimationUtility.CalculateTransformPath(go.transform, null);

					// NOTE: We add '/' to the start of the path as without this GameObject.Find will find any GameObject with this name and not start from scene root
					result += "/" + path + '*';

				}
			}
			return result;
		}

		public static string GetSelectionAsString()
		{
			string result = string.Empty;
			// Cache the selection
			if (Selection.activeGameObject != null)
			{
				// Convert transforms into a path list separated by '*' character
				if (Selection.transforms != null && Selection.transforms.Length > 0)
				{
					for (int i = 0; i < Selection.transforms.Length; i++)
					{
						Transform xform = Selection.transforms[i];
						string path = AnimationUtility.CalculateTransformPath(xform, null);

						// NOTE: We add '/' to the start of the path as without this GameObject.Find will find any GameObject with this name and not start from scene root
						result += "/" + path + '*';
					}
				}
			}
			return result;
		}

		public static void SetHierarchyFromString(string list, bool logMissingNodes)
		{
			string[] paths = null;
			{
				// Convert the string list to an array
				if (!string.IsNullOrEmpty(list))
				{
					paths = list.Split(new char[] { '*' }, System.StringSplitOptions.RemoveEmptyEntries);
				}
			}

			// Convert the paths array into an array of objects
			GameObject[] gos = null;
			if (paths != null && paths.Length > 0)
			{
				List<GameObject> goList = new List<GameObject>();
				foreach (string path in paths)
				{
					// TODO: GameObject.Find doesn't find disabled objects, so we need to handle this
					GameObject go = GameObject.Find(path);
					if (go != null)
					{
						goList.Add(go);
					}
					else if (logMissingNodes)
					{
						Debug.LogWarning("[HierarchyRestore] Can't find node with path: '" + path + "'");
					}
				}
				gos = goList.ToArray();
			}

			if (gos != null && gos.Length > 0)
			{
				HierarchyUtils.SetHierarchyWindowExpandedObjects(gos);
			}
			else
			{
				HierarchyUtils.SetHierarchyWindowExpandedObjects(new GameObject[0]);
			}
		}

		public static bool SetSelectionFromString(string list, bool logMissingNodes)
		{
			// Get the path list and convert it to an array
			string[] paths = null;
			if (!string.IsNullOrEmpty(list))
			{
				paths = list.Split(new char[] { '*' }, System.StringSplitOptions.RemoveEmptyEntries);
			}

			// Convert the paths array into a list of objects
			List<Object> gos = new List<Object>();
			if (paths != null && paths.Length > 0)
			{
				foreach (string path in paths)
				{
					// TODO: GameObject.Find doesn't find disabled objects, so we need to handle this
					GameObject go = GameObject.Find(path);
					if (go != null)
					{
						gos.Add(go);
					}
					else if (logMissingNodes)
					{
						Debug.LogWarning("[HierarchyRestore] Can't find node with path: '" + path + "'");
					}
				}
			}

			if (gos.Count > 0)
			{
				Selection.objects = gos.ToArray();
			}

			return (gos.Count > 0);
		}

		private static bool IsHierarchyWindow(EditorWindow window)
		{
			bool result = false;
			if (window != null)
			{
#if RH_UNITY_FEATURE_WINDOWTITLE
				if (window.titleContent.text == "Hierarchy")
#else
				if (window.title == "UnityEditor.HierarchyWindow")
#endif
				{
					if (window.GetType().Name == "SceneHierarchyWindow")
					{
						result = true;
					}
				}
			}
			return result;
		}

		private static EditorWindow GetHierarchyWindow()
		{
			// Search from focused window
			EditorWindow hierarchyWindow = EditorWindow.focusedWindow;
			if (!IsHierarchyWindow(hierarchyWindow))
			{
				hierarchyWindow = null;
			}

			// Search by type (we don't do this because it creates the window if one isn't open...)
			/*if (hierarchyWindow == null)
			{
				if (_hierarchyWindowType == null)
				{
					System.Reflection.Assembly editorAssembly = typeof(EditorWindow).Assembly;
					if (editorAssembly != null)
					{
						_hierarchyWindowType = editorAssembly.GetType("UnityEditor.SceneHierarchyWindow");
					}
				}
				if (_hierarchyWindowType != null)
				{
					hierarchyWindow = EditorWindow.GetWindow(_hierarchyWindowType);
					if (!IsHierarchyWindow(hierarchyWindow))
					{
						hierarchyWindow = null;
					}
				}
			}*/


			// Search by iterating all editor windows (needed?)
			if (hierarchyWindow == null)
			{
				EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
				//Debug.Log("searching windows " + windows.Length);
				for (int i = 0; i < windows.Length; i++)
				{
					if (IsHierarchyWindow(windows[i]))
					{
						hierarchyWindow = windows[i];
						//Debug.Log("found by searching");
						break;
					}
				}
			}


			// Old way to find the hierarchy window - the problem is it opens the window if it isn't visible, which can be undesirable
			/*
			EditorApplication.ExecuteMenuItem("Window/Hierarchy");
			EditorWindow hierarchyWindow = EditorWindow.focusedWindow;
			*/

			return hierarchyWindow;
		}

		private static object _cachedTreeViewDataSource = null;
		private static System.Reflection.MethodInfo _cachedMethodGetExpandedIds = null;
		private static System.Reflection.MethodInfo _cachedMethodSetExpandedIds = null;

		// Returns a list of GameObjects that are expanded in the Hierarchy window
		public static GameObject[] GetHierarchyWindowExpandedObjects()
		{
			List<GameObject> result = new List<GameObject>();

			try
			{
				if (_cachedTreeViewDataSource == null || _cachedMethodGetExpandedIds == null)
				{
					// NOTE: treeView could be TreeView or TreeViewController depending on the Unity version
					object treeView = null;
					{
						// Get the hierarchy window
						EditorWindow hierarchyWindow = GetHierarchyWindow();
						if (hierarchyWindow == null)
						{
							return result.ToArray();
						}

						// Get the TreeViewState
						var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
						if (type != null)
						{
							var propInfo = type.GetProperty("treeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
							treeView = propInfo.GetValue(hierarchyWindow, null);
						}
					}

					object treeViewDataSource = null;
					if (treeView != null)
					{
						// Get the TreeViewState
						var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.TreeView");
						if (type == null)
						{
							type = typeof(EditorWindow).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");
						}
						if (type != null)
						{
							var propInfo = type.GetProperty("data", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
							treeViewDataSource = propInfo.GetValue(treeView, null);
						}
					}

					// Get the list of expanded instanceIDs
					if (treeViewDataSource != null)
					{
						var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.TreeViewDataSource");
						if (type == null)
						{
							// In newer versions of Unity TreeViewDataSource has moved here:
							type = typeof(EditorWindow).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewDataSource");
						}

						_cachedTreeViewDataSource = treeViewDataSource;
						_cachedMethodGetExpandedIds = type.GetMethod("GetExpandedIDs", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
					}
				}
				else
				{
					//Debug.Log("Using cached...");
				}

				int[] expandedIds = null;
				if (_cachedTreeViewDataSource != null && _cachedMethodGetExpandedIds != null)
				{
					expandedIds = (int[])_cachedMethodGetExpandedIds.Invoke(_cachedTreeViewDataSource, null);
				}

				// Filter the list for GameObjects
				if (expandedIds != null)
				{
					foreach (int id in expandedIds)
					{
						GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
						if (go != null)
						{
							result.Add(go);
						}
					}
				}
				else
				{
					Debug.LogWarning("[HierarchyRestore] Can't hierarchy get methods via reflection");
				}
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}

			return result.ToArray();
		}

		public static void SetHierarchyWindowExpandedObjects(GameObject[] gos)
		{
#if RH_UNITY_FEATURE_DEBUG_ASSERT
			Debug.Assert(gos != null);
#endif
			try
			{
				if (_cachedTreeViewDataSource == null || _cachedMethodSetExpandedIds == null)
				{
					// NOTE: treeView could be TreeView or TreeViewController depending on the Unity version
					object treeView = null;
					{
						// Get the hierarchy window
						EditorWindow hierarchyWindow = GetHierarchyWindow();
						if (hierarchyWindow == null)
						{
							return;
						}

						// Get the TreeView
						var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
						if (type != null)
						{
							var propInfo = type.GetProperty("treeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
							if (propInfo != null)
							{
								treeView = propInfo.GetValue(hierarchyWindow, null);
							}
						}
					}

					object treeViewDataSource = null;
					if (treeView != null)
					{
						// Get the TreeViewState
						var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.TreeView");
						if (type == null)
						{
							// In newer versions of Unity TreeViewState has moved here:
							type = typeof(EditorWindow).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");
						}
						if (type != null)
						{
							var propInfo = type.GetProperty("data", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
							treeViewDataSource = propInfo.GetValue(treeView, null);
						}
					}

					// Set the expanded items in the hierachy window
					if (treeViewDataSource != null)
					{
						var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.TreeViewDataSource");
						if (type == null)
						{
							// In newer versions of Unity TreeViewDataSource has moved here:
							type = typeof(EditorWindow).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewDataSource");
						}

						_cachedTreeViewDataSource = treeViewDataSource;
						_cachedMethodSetExpandedIds = type.GetMethod("SetExpandedIDs", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
					}
				}
				else
				{
					//Debug.Log("Using cached...");
				}

				if (_cachedTreeViewDataSource != null && _cachedMethodSetExpandedIds != null)
				{
					// Convert array of GameObjects to array of instanceIDs
					int[] ids = new int[gos.Length];
					{
						for (int i = 0; i < gos.Length; i++)
						{
							ids[i] = gos[i].GetInstanceID();
						}
					}

					_cachedMethodSetExpandedIds.Invoke(_cachedTreeViewDataSource, new object[] { ids });
				}
				else
				{
					Debug.LogWarning("[HierarchyRestore] Can't hierarchy set methods via reflection");
				}
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
#endif