# quick script to test math for spread algorithm

import math, random, csv

samples = 10000000
res = 10
output_path = 'sampled.csv'
output_file = open(output_path, 'w')
csvwriter = csv.writer(output_file, lineterminator = '\n')
#data = [[0] * 2 for i in range(samples)]
data = [(0, 0)] * samples

print("generating points...")

for i in range(0, samples):
    radius = random.random()
    theta = random.random() * math.pi * 2
    x = radius * math.cos(theta)
    y = radius * math.sin(theta)
    #data.append((x, y))
    data[i] = (x, y)
    #csvwriter.writerow((x, y))

print("categorizing points...")

sample_counts = [[0] * (2 * res + 1) for i in range(2 * res + 1)]
pixel_half = .5 / res
    
for point in data:
    x = point[0]
    y = point[1]
    x_index = int(x * res + .5 + res)
    y_index = int(y * res + .5 + res)
    #print("{0:.3f}\t{1:.3f}\t{2}\t{3}".format(x, y, x_index, y_index))
    #if x_index >= 0 and y_index >= 0:
    sample_counts[y_index][x_index] += 1

print("writing file...")

for row in sample_counts:
    csvwriter.writerow(row)
output_file.close()

print("done!")
