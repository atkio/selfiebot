from __future__ import (absolute_import, division, print_function,
                        unicode_literals)

import json
import os
import pathlib
import shutil
import threading
import time
from os import listdir, walk
from os.path import isfile, join
from pathlib import Path

import numpy as np
import tensorflow as tf
import tensorflow_hub as hub
from absl import app, flags, logging
from tensorflow import keras

PATH_TEMP = "./TEMP"
PATH_RT = "./RT"
PATH_DEL = "./DEL"
PATH_SEX = "./SEX"
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


def model1action(path, result):
    path = ''.join([chr(e) for e in path])
    resultlist = result.astype('str')
    print(path, ":", resultlist)
    if "Girl" in resultlist and "Human face" in resultlist and not "Man" in resultlist:
        shutil.move(path, PATH_RT)
    else:
        shutil.move(path, PATH_DEL)


detector1 = hub.load(PATH_MODEL1).signatures['default']
list_ds = tf.data.Dataset.from_tensor_slices(
    listimage(PATH_TEMP)).map(tf_load_images, num_parallel_calls=128)
for converted_img, path in list_ds:
    result = detector1(converted_img)['detection_class_entities'].numpy()
    path = path.numpy()
    threading.Thread(target=model1action, args=(path, result)).start()

time.sleep(120)


model = tf.keras.models.load_model(PATH_MODEL2)
images, image_paths = keras_load_images(PATH_RT)
model_preds = model.predict(images)

#categories = ['drawings', 'hentai', 'neutral', 'porn', 'sexy']
for i, single_preds in enumerate(model_preds):
    if float(single_preds[3]) > 0.7 or float(single_preds[1]) > 0.7:
        shutil.move(image_paths[i], PATH_PORN)
    elif float(single_preds[4]) > 0.7:
        shutil.move(image_paths[i], PATH_SEX)
    elif float(single_preds[0]) > 0.7:
        shutil.move(image_paths[i], PATH_DEL)
