import os
from ctypes import windll, c_void_p, c_int
import tkinter as tk
from tkinter import messagebox

# Load the DLL
dll_path = os.path.join(os.getcwd(), "USBaccessX64.dll")
print(f"Loading DLL from: {dll_path}")
mydll = windll.LoadLibrary(dll_path)

# Set the return type of FCWInitObject to c_void_p (pointer to void)
mydll.FCWInitObject.restype = c_void_p

# Initialize the Cleware object
cw = mydll.FCWInitObject()

if not cw:
    raise RuntimeError("Failed to initialize Cleware object")

# Convert the integer handle to a pointer
cw_pointer = c_void_p(cw)

# Open Cleware devices
devCnt = mydll.FCWOpenCleware(cw_pointer)
print(f"Found {devCnt} devices")

if devCnt <= 0:
    raise RuntimeError("No devices found")

# Assume the first device is the switch device
switch_index = 0
switch_id = 0x10  # Typically SWITCH_0, adjust according to your setup

# Function to turn on the switch
def turn_on_switch():
    result = mydll.FCWSetSwitch(cw_pointer, switch_index, switch_id, c_int(1))  # 1 = ON
    if result > 0:
        messagebox.showinfo("Switch Control", "Switch turned ON successfully")
    else:
        messagebox.showerror("Switch Control", f"Failed to turn ON the switch, error code: {result}")

# Function to turn off the switch
def turn_off_switch():
    result = mydll.FCWSetSwitch(cw_pointer, switch_index, switch_id, c_int(0))  # 0 = OFF
    if result > 0:
        messagebox.showinfo("Switch Control", "Switch turned OFF successfully")
    else:
        messagebox.showerror("Switch Control", f"Failed to turn OFF the switch, error code: {result}")

# GUI setup
root = tk.Tk()
root.title("USB Switch Control")

# Create buttons
btn_on = tk.Button(root, text="Turn ON", command=turn_on_switch, width=20, height=2)
btn_on.pack(pady=10)

btn_off = tk.Button(root, text="Turn OFF", command=turn_off_switch, width=20, height=2)
btn_off.pack(pady=10)

# Start the GUI loop
root.mainloop()

# Clean up after the GUI is closed
mydll.FCWCloseCleware(cw_pointer)
mydll.FCWUnInitObject(cw_pointer)
