import sys


def getLen(x, y):
    ratio = gcd(float(x), float(y))
    height = 100
    width = ratio[0] * (height/ratio[1])
    print (width, height)

def gcd(x, y, div=2.0):
    print (x, y)

    if x % div != 0.0 or y % div != 0.0:
        if div == 6:
            return (x, y)
        else:
            return gcd(x, y, div+1)
    else:
        return gcd(x/div, y/div)


def gcd2(x, y):

    noDivs = False
    divisor = 1.0

    while not noDivs:
        divisor += 1.0
        if x % divisor == 0.0 and y % divisor == 0.0:
            x = x/divisor
            y = y/divisor
            divisor = 1.0
        elif divisor == 11.0:
            noDivs = True

    return (x, y)


getLen(float(sys.argv[1]), float(sys.argv[2]))


