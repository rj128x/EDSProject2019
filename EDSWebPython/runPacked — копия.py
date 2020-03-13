import config
import datetime
import time
from webAPI import EdsWebApi


#Скрипт для определения всех изменений бита пакетной точки за заданный интервал
web=EdsWebApi()
web.connect()
web.logger.debug('lalalala')

ds=datetime.datetime(2019,3,1)
de=datetime.datetime(2019,4,25)
#tag='62VT_UB1B8T-FP.UNIT0@SCADA:B1' 
tag='72VT_SP03SP-005.UNIT0@SCADA' 
bit=3




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
				print('{0} - {1} [{2}]'.format(onDT,offDT,offDT.timestamp()-onDT.timestamp())) #Выодим полученный результат на экран
	else:
		dt=d1
print('finish')
input()
