import scipy.misc
import numpy as np
import os
import matplotlib.pyplot as plt


percentagesLimit = [0.01, 0.03, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30]
#percentagesLimit = [0.01, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.40, 0.50, 0.60, 0.70, 0.80, 0.90, 1.00]

def getIntensityOfPixel(vector, maximum = 255):
    if type(vector) is not np.ndarray:
        vector = [vector]

    vectorLen = len(vector)
    totalIntensity = 0.0
    for v in vector:
        totalIntensity += float(v)
    relativeIntensity = totalIntensity / (vectorLen * maximum)
    return relativeIntensity

def getIntenitiesInTruth(path):
    nonEmptyPoints = []
    numpyArray = scipy.misc.imread(path)
    for i in range(numpyArray.shape[0]):
        for j in range(numpyArray.shape[1]):
            if numpyArray[i, j].any() != 0:
                nonEmptyPoints.append([i, j])
    return nonEmptyPoints

def getIntensitiesResult(path, truths):
    numpyArray = scipy.misc.imread(path)
    intensities = []
    for coordinates in truths:
        x = coordinates[0]
        y = coordinates[1]
        if numpyArray.shape[0] <= x or numpyArray.shape[1] <= y:
            print('x'+str(x)+' y'+str(y)+' Shape '+str(numpyArray.shape[0])+' '+str(numpyArray.shape[1]))
        else:
            intensity = getIntensityOfPixel(numpyArray[x, y])
            intensities.append(intensity)
    return intensities

def getImagePaths(folderPath):
    return os.listdir(folderPath)

def getAllIntensities():
    resultFolderPath = 'results/'
    resultPaths = getImagePaths(resultFolderPath)
    truthFolderPath = 'truths/'

    allIntensities = []

    for path in resultPaths:
        truthIntensity = getIntenitiesInTruth(truthFolderPath + path)
        allIntensities.append(getIntensitiesResult(resultFolderPath + path, truthIntensity))

    return allIntensities

def getPercentages(results):
    percentagesLen = len(percentagesLimit)
    totalPercentages = [0] * percentagesLen

    for res in results:
        total = float(len(res))
        if total == 0:
            print("What")
            continue
        tempPercentages = [0] * percentagesLen
        for r in res:
            for index in range(percentagesLen):
                if r <= percentagesLimit[index]:
                    tempPercentages[index] += 1

        for i in range(percentagesLen):
            totalPercentages[i] += tempPercentages[i]/total

    return totalPercentages


def create_graph(percentages):
    print(percentages)
    resultFolderPath = 'results/'
    resultPaths = getImagePaths(resultFolderPath)
    numberOfImages = len(resultPaths)
    percentages = [x/numberOfImages for x in percentages]
    print(percentages)
    scale = [x*100 for x in percentagesLimit]
    plt.plot(scale, percentages, '-ro')
    plt.xlim(0, percentagesLimit[-1] * 100)
    plt.ylim(0, 1)
    plt.xlabel('Percent Salient')
    plt.ylabel('True Positive Rate')
    plt.show()



def get_average_percentage():
    saliences = [[0.05379062695818804, 0.22782625715119592, 0.38411090885186683, 0.5003764685797745, 0.5929397569246241, 0.6646945032267988, 0.7311870861455677, 0.8305155026014186, 0.8959843115918901, 0.9422794874747523, 0.9693904212429523, 0.9875625425722512, 0.9967460637850929, 1.0]
, [0.39978003612712054, 0.6598889417318714, 0.7725057018357923, 0.8353959064070391, 0.8772724864785381, 0.9052002968628317, 0.9276345036608289, 0.9579127533759478, 0.9756955323716264, 0.9873607841754904, 0.9938567175419497, 0.9976772607872678, 0.9994791488900009, 1.0]
, [0.03139108949056719, 0.18454689171964464, 0.3385451657873674, 0.45909582247341235, 0.5580079089788856, 0.6371095380648748, 0.7081768356244678, 0.8153004241970258, 0.8854309920116582, 0.9354972374782825, 0.9660376993857265, 0.9862935934889648, 0.9962908413089536, 1.0]
, [0.04484944089754066, 0.2137897370437296, 0.37096845776017795, 0.48917181104327195, 0.5829888664295979, 0.6583689648830926, 0.7273340235096317, 0.8285704661111761, 0.8945672661799734, 0.9416787102191171, 0.9693279265004018, 0.9876554939788993, 0.9967559621510939, 1.0]
]
    averages = []
    for i in range(len(saliences[0])):
        sum = 0.0
        for j in range(len(saliences)):
            sum += float(saliences[j][i]) * 58.0
        averages.append(sum / float(len(saliences)))
    return averages

def plot_average():
    averages = get_average_percentage()
    create_graph(averages)

def main():
    aa = getAllIntensities()
    bb = getPercentages(aa)
    create_graph(bb)
    #create_graph([0.002951810298677361, 0.011402438622797561, 0.05940129155327934, 0.1342430329219704, 0.24063228779137766, 0.34888912697856483, 0.4665842900130221, 0.6779445337424014, 0.8162810908881211, 0.9033161592241516, 0.9509621267391937, 0.9801178985172346, 0.9937547123958813, 1.0])


main()
#plot_average()