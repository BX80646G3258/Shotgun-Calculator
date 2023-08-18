import os

extension = '.smd'
new_extension = '.txt'

def rename_recursive(path):
    for file in os.listdir(path):
        filepath = path + '/' + file
        if os.path.isdir(filepath):
            rename_recursive(filepath)
        elif file.endswith(extension):
            try:
                os.rename(filepath, filepath[0:-len(extension)] + new_extension)
            except FileExistsError:
                print(filepath + ' already exists!')
            
rename_recursive('.')
