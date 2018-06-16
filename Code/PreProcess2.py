import os
from shutil import copyfile

truth_path = 'ALLFIXATIONMAPS/ALLFIXATIONMAPS/'
input_path = 'Input/'
output_path = 'Truth/'

def getImagePaths(folderPath):
    return os.listdir(folderPath)

input_names = getImagePaths(input_path)

for input_name in input_names:

    before_file = input_name.split('.')[0]

    truth_name = input_name

    src = truth_path + truth_name
    dst = output_path + input_name

    copyfile(src, dst)







