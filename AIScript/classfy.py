from __future__ import (absolute_import, division, print_function,
                        unicode_literals)

import shutil
from os import listdir
from os.path import isfile, join

import numpy as np
import tensorflow_hub as hub
import tensorflow as tf
from tensorflow import keras

PATH_TEMP = "./TEMP"
PATH_RT = "./RT"
PATH_DEL = "./DEL"
PATH_SEXY = "./SEXY"
PATH_PORN = "./PORN"

PATH_MODEL1 = "AIScript/model/facemodel"
PATH_MODEL2 = "AIScript/model/mobilenet_v2_140_224"


def listimage(image_paths):
    parent = image_paths
    return [join(parent, f) for f in listdir(image_paths)
            if isfile(join(parent, f)) and f.lower().endswith(('.png', '.jpg', '.jpeg'))]


def tf_load_images(filepath):
    img = tf.io.read_file(filepath)
    img = tf.image.decode_jpeg(img, channels=3)
    return tf.image.convert_image_dtype(img, tf.float32)[
        tf.newaxis, ...], filepath


def keras_load_images(path):
    loaded_images = []
    loaded_image_paths = []
    image_paths = listimage(path)
    for i, img_path in enumerate(image_paths):
        try:
            image = keras.preprocessing.image.load_img(
                img_path, target_size=(224, 224))
            image = keras.preprocessing.image.img_to_array(image)
            image /= 255
            loaded_images.append(image)
            loaded_image_paths.append(img_path)
        except Exception as ex:
            print(i, img_path, ex)

    return np.asarray(loaded_images), loaded_image_paths


# 第一批处理 TEMP中分类
temp_images = listimage(PATH_TEMP)
if len(temp_images) < 1:
    exit()

detector1 = hub.load(PATH_MODEL1).signatures['default']
list_ds = tf.data.Dataset.from_tensor_slices(
    temp_images).map(tf_load_images, num_parallel_calls=128)

detected_images = []
detected_image_paths = []
for converted_img, path in list_ds:
    detected_images.append(detector1(converted_img)[
                           'detection_class_entities'].numpy())
    detected_image_paths.append(path.numpy())

for i in range(len(detected_images)):
    path = ''.join([chr(e) for e in detected_image_paths[i]])
    resultlist = detected_images[i].astype('str')
    # print(path, ":", resultlist)
    if "Girl" in resultlist and "Human face" in resultlist and "Man" not in resultlist:
        shutil.move(path, PATH_RT)
    else:
        shutil.move(path, PATH_DEL)

# 第二批处理 RT中再分类
images, image_paths = keras_load_images(PATH_RT)
if len(images) < 1:
    exit()

model = tf.keras.models.load_model(PATH_MODEL2)
model_preds = model.predict(images)

#categories = ['drawings', 'hentai', 'neutral', 'porn', 'sexy']
for i, single_preds in enumerate(model_preds):
    if float(single_preds[3]) > 0.7 or float(single_preds[1]) > 0.7:
        shutil.move(image_paths[i], PATH_PORN)
    elif float(single_preds[4]) > 0.7:
        shutil.move(image_paths[i], PATH_SEXY)
    elif float(single_preds[0]) > 0.7:
        shutil.move(image_paths[i], PATH_DEL)
