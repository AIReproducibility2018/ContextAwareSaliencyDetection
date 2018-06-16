
import os
from random import shuffle
from shutil import copyfile


def getImagePaths(folderPath):
    return os.listdir(folderPath)


input_path = 'Input/'
output_path = 'Output/'

files = getImagePaths(input_path)

shuffle(files)

for i in range(100):
    file_name = files[i]

    src = input_path + file_name
    dst = output_path + file_name

    copyfile(src, dst)




