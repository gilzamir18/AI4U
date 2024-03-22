# Known Issues

1. We have a setup problem in Windows 11 with Python 3.10.11 installed from the Microsoft Store. The comand "pip install -e ." run, but when we import ai4u, ones received message "ModuleNotFound: No module named ai4u". This problem only happens when pip is upgraded from version '23.*' to version '24.*'.