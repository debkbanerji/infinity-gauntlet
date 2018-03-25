import json
import uuid
import os
import thread
import random
from os import listdir
from os.path import isfile, join

import pyrebase

import RPi.GPIO as GPIO  # Import GPIO library
import time  # Import time library

script_dir = os.path.dirname(__file__)

configFile = open(os.path.join(script_dir, 'firebase-config.json'), 'r+')
config = json.load(configFile)

firebase = pyrebase.initialize_app(config)

db = firebase.database()

# storage = firebase.storage()

this_hand_id = "hand_1"

hand_path = "hands/" + this_hand_id
curr_finger_path = hand_path + "/curr_finger"
hand_status_path = hand_path + "/hand_status"

GPIO.setmode(GPIO.BOARD)  # Set GPIO pin numbering
GPIO.setwarnings(False)

INIT_PIN = 12
NON_IDLE_PIN = 16

GPIO.setup(INIT_PIN, GPIO.OUT)
GPIO.setup(NON_IDLE_PIN, GPIO.OUT)

GPIO.output(INIT_PIN, GPIO.LOW)
time.sleep(2)
show_light_ref = db.child("show_light").get()
GPIO.output(INIT_PIN, GPIO.HIGH)
print("Starting Finger Listener")

FINGER_PINS = [15, 13, 11, 7]
FINGER_LIGHT_PINS = [35, 33, 31, 29]

for FINGER_PIN in FINGER_PINS:
    GPIO.setup(FINGER_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)

for FINGER_LIGHT_PIN in FINGER_LIGHT_PINS:
    GPIO.setup(FINGER_LIGHT_PIN, GPIO.OUT)

old_finger_pin_inputs = []

while True:

    finger_pin_inputs = [not GPIO.input(FINGER_PIN) for FINGER_PIN in FINGER_PINS]

    finger_pin_input_numbers = [1 if finger_pin_input else 0 for finger_pin_input in finger_pin_inputs]

    for i in range(0, len(finger_pin_inputs)):
	if finger_pin_inputs[i]:
            GPIO.output(FINGER_LIGHT_PINS[i], GPIO.HIGH)
	else:
	    GPIO.output(FINGER_LIGHT_PINS[i], GPIO.LOW)

    if not old_finger_pin_inputs == finger_pin_inputs:
        curr_finger = "idle"
        if finger_pin_inputs[0]:
            curr_finger = "index"
        elif finger_pin_inputs[1]:
            curr_finger = "middle"
        elif finger_pin_inputs[2]:
            curr_finger = "ring"
        elif finger_pin_inputs[3]:
            curr_finger = "pinky"

	print(str(finger_pin_inputs))
	if finger_pin_inputs[1] and finger_pin_inputs[2]:
            curr_finger = "tap"

        if sum(finger_pin_input_numbers) >= 4:
            curr_finger = "all"   
        
        db.child(hand_status_path).set(finger_pin_inputs)
        db.child(curr_finger_path).set(curr_finger)
    
    if sum(finger_pin_input_numbers) > 0:
        GPIO.output(NON_IDLE_PIN, GPIO.HIGH)
    else:
        GPIO.output(NON_IDLE_PIN, GPIO.LOW)
 
    old_finger_pin_inputs = list(finger_pin_inputs)
