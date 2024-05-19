#!/bin/bash
cd ./Python || return
jupyter notebook --no-browser --ip='*' --NotebookApp.token='' --NotebookApp.password=''
