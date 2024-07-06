from setuptools import setup, find_packages

with open("README.md", "r") as fh:
    long_description = fh.read()

setup(name='ai4u',
        version='0.1.0',
        author="Gilzamir Gomes",  
        description="AI4U connect Godot and python applications compatible with gynasium",
        long_description=long_description,  
        long_description_content_type="text/markdown",
        packages=find_packages(),
        classifiers=[
                "Programming Language :: Python :: 3.10",
                "License :: OSI Approved :: MIT License",
                "Operating System :: OS Independent",
        ],
        python_requires='>=3.10',
        install_requires=['numpy==1.26.4', 'gymnasium==0.29.1', 'stable-baselines3==2.3.2', 'torch==2.2.2', 'tensorboard==2.16.2', 'shimmy==1.3.0'],
        py_modules=['ai4u', 'AI4UEnv']
)
