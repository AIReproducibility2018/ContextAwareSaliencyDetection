import scipy.misc
import numpy as np
import os
import matplotlib.pyplot as plt

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

def getAllIntensities(truthFolderPath):
    resultFolderPath = 'Output/'
    resultPaths = getImagePaths(resultFolderPath)

    allIntensities = []

    for path in resultPaths:
        truthIntensity = getIntenitiesInTruth(truthFolderPath + path)
        allIntensities.append(getIntensitiesResult(resultFolderPath + path, truthIntensity))

    return allIntensities

def getPercentages(results, percentagesLimit):
    percentagesLen = len(percentagesLimit)
    totalPercentages = [0] * percentagesLen

    for res in results:
        total = float(len(res))
        tempPercentages = [0] * percentagesLen
        for r in res:
            for index in range(percentagesLen):
                if r <= percentagesLimit[index]:
                    tempPercentages[index] += 1

        for i in range(percentagesLen):
            totalPercentages[i] += tempPercentages[i]/total

    return totalPercentages

def create_graph(percentages, percentagesLimit):
    print(percentages)
    resultFolderPath = 'Output/'
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

def get_average_percentage(saliences):
    averages = []
    for i in range(len(saliences[0])):
        sum = 0.0
        for j in range(len(saliences)):
            sum += float(saliences[j][i]) * 58.0
        averages.append(sum / float(len(saliences)))
    return averages

def ASpectralResidualApproach():
	percentagesLimit = [0.01, 0.03, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30]
	pathToTruth = 'cvpr07supp/hMap/'
	subfolders = ['a1', 'a2', 'a3', 'a4']
	percentages = []
	for subfolder in subfolders:
		intensities = getAllIntensities(pathToTruth+'a1/')
		percentages.append(getPercentages(intensities, percentagesLimit))
	averagePercentages = get_average_percentage(percentages)
    create_graph(averagePercentages, percentagesLimit)
	
def LearningToPredictWhereHumansLook():
	percentagesLimit = [0.01, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.40, 0.50, 0.60, 0.70, 0.80, 0.90, 1.00]
	pathToTruth = 'Truth'
    intensities = getAllIntensities(pathToTruth)
	percentages = getPercentages(intensities, percentagesLimit)
	create_graph(percentages, percentagesLimit)
	
#ASpectralResidualApproach()
#LearningToPredictWhereHumansLook()