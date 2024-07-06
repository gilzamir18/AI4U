from setuptools import setup, find_packages

setup(name='ai4u',
        version='0.1.0',
        install_requires=['numpy==1.26.4', 'gymnasium==0.29.1', 'stable-baselines3==2.3.2', 'torch==2.2.2', 'tensorboard==2.16.2', 'shimmy==1.3.0'],
        py_modules=['ai4u', 'AI4UEnv']
)
