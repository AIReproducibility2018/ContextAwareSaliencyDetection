import os
from shutil import copyfile

truth_path = 'Database 17/Output/'
input_path = 'Input/'
output_path = 'Output/'

def getImagePaths(folderPath):
    return os.listdir(folderPath)

input_names = getImagePaths(input_path)

for input_name in input_names:

    before_file = input_name.split('.')[0]

    truth_name = input_name#before_file + '_fixPts.jpg'

    src = truth_path + truth_name
    dst = output_path + input_name

    copyfile(src, dst)







