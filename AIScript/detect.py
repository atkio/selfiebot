from __future__ import absolute_import, division, print_function, unicode_literals

# For drawing onto the image.
import threading
import shutil
import os
import numpy as np

import pathlib
# For running inference on the TF-Hub module.
import tensorflow as tf
import tensorflow_hub as hub

detector2 = hub.load("./AIScript/model/facemodel").signatures['default']


def parse(path):
    img = tf.io.read_file(path)
    img = tf.image.decode_jpeg(img, channels=3)
    converted_img = tf.image.convert_image_dtype(img, tf.float32)[
        tf.newaxis, ...]
    return path, converted_img


def fileaction(path, resultlist):
    path = ''.join([chr(e) for e in path])
    if "Girl" in resultlist and "Human face" in resultlist and not "Man" in resultlist:
        shutil.move(path, "./RT/")
    else:
        shutil.move(path, "./DEL/")


image_root = pathlib.Path("./TEMP")
list_ds = tf.data.Dataset.list_files(
    str(image_root/'*.*')).map(parse, num_parallel_calls=128)
for path, converted_img in list_ds:
    result = detector2(converted_img)[
        'detection_class_entities'].numpy().astype('str')
    threading.Thread(target=fileaction, args=(path.numpy(), result)).start()
