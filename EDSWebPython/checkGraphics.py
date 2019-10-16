import config
import datetime
import time
import os
import re

from webAPI import EdsWebApi


#Скрипт для определения всех изменений бита пакетной точки за заданный интервал
web=EdsWebApi()
web.connect()
web.logger.debug('lalalala')

directory = 'c:/Users/chekunovamv/AppData/Roaming/EDS/9.1/terminal/objects/Graphics'
dirRe="[A-Za-z0-9-]*.edf"
#pointRe="11VT_[A-Za-z0-9-]{3,15}[.]{1}[A-Za-z0-9]{3,10}[@]{1}[A-Za-z0-9]{3,10}"
#pointRe="07VT_[A-Za-z0-9-]{3,15}[.]{1}MCR[@]{1}GRARM"
pointRe="[0-1]{1}[0-9]{1}VT_[A-Za-z0-9-]{3,15}[.]{1}[A-Za-z0-9]{3,10}[@]{1}[A-Za-z0-9]{3,10}"
files = os.listdir(directory)
result={}
for fileName in files:
        match=re.match(dirRe,fileName)
        if (match==None):
                continue
        
        fullFileName='{0}/{1}'.format(directory,fileName)
        f = open(fullFileName, 'r')
        text=f.read()
        match=re.findall(pointRe,text)        

        for point in match:
                if not (point in result):
                        result[point]=fileName
                else:
                        result[point]=result[point]+" "+fileName
                        
        
for point in result:
        print('{0};{1}'.format(point,result[point]))
