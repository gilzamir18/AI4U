# Known Issues

## Problem: ValueError: unsupported pickle protocol: 5

The solution to this problem can be obtained in two ways:

1) upgrading the Python version to a version that supports the g pickle 5 protocol:

```
pip install --upgrade python
```
.

2) If it is not possible to update the python interpreter version, install the specified protocol:

```
pip install pickle5
```

