from PIL import Image
import os


def getImagePaths(folderPath):
    return os.listdir(folderPath)

truth_path = 'Truths/'
result_path = 'Results/'
names = getImagePaths(result_path)

for name in names:
    truth_image = Image.open(truth_path + name)
    basewidth = truth_image.width
    img = Image.open(result_path + name)
    wpercent = (basewidth/float(img.size[0]))
    hsize = int((float(img.size[1])*float(wpercent)))
    img = img.resize((basewidth,hsize), Image.ANTIALIAS)
    img = img.convert("RGB")
    img.save(result_path + name)