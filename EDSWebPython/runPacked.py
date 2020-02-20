import config
import datetime
import time
from webAPI import EdsWebApi


#Скрипт для определения всех изменений бита пакетной точки за заданный интервал
web=EdsWebApi()
web.connect()
web.logger.debug('lalalala')

ds=datetime.datetime(2019,10,9)
de=datetime.datetime(2019,10,12)


#tag='62VT_UB1B8T-FP.UNIT0@SCADA:B1'




tag='72VT_SP03SP-005.UNIT0@SCADA' 
bit=3


def getData(tag,bit,tag2):
        print(tag)
        dt=ds
        data=[]
        val=web.getVal(ds,tag)#Получаем значение точки
        bitVal=web.getBitVal(val,bit)#Получаем значения бита 
        print(bitVal)
        #val=0.1
        #bitVal=True
        onDT=None
        offDT=None
        if bitVal:#Если бит взведен, то инициализируем значение точки onDT
                onDT=ds
        while dt<de:
                if (not bitVal):
                        d1=web.getDateChange(dt,de,tag,"F_INTOOVER_DT",2**bit)
                else:
                        d1=web.getDateChangeFull(dt,de,tag,val)#Получаем следующее изменение пакетной точки (как целочисленной величины)
                valNew=web.getVal(d1,tag)#Получаем значение точки в этот момент
                bitValNew=web.getBitVal(valNew,bit)#Получаем значение бита в данный момент
                if val!=valNew:#Если значение пакетной точки изменилось обновляем дату начала выборки
                        val=valNew
                        dt=d1
                        if bitVal != bitValNew: #Определяем, изменился ли нужный нам бит
                                bitVal=bitValNew
                                if bitVal:
                                        onDT=d1
                                else:
                                        offDT=d1
                                        val2=web.getVal(d1,tag2)
                                        print('{0} - {1} [{2}] {3}'.format(onDT,offDT,offDT.timestamp()-onDT.timestamp(),val2)) #Выодим полученный результат на экран
                else:
                        dt=d1


#getData('07VT_D007P1B4S3.MCR@GRARM',15,'05VT_GC01A-16.MCR@GRARM')
#getData('07VT_D007P1B4S4.MCR@GRARM',15,'05VT_GC01A-16.MCR@GRARM')
getData('05VT_D007P1B4S4.MCR@GRARM',15,'05VT_GC01A-16.MCR@GRARM')
#getData('05VT_D007P1B4S5.MCR@GRARM',15,'05VT_GC01A-16.MCR@GRARM')
getData('05VT_D007P1B4S6.MCR@GRARM',15,'05VT_GC01A-16.MCR@GRARM')



print('finish')
input()
