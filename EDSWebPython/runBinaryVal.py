import config
import datetime
import time
from webAPI import EdsWebApi

#Простой сценарий для получения всех изменений дискретной точки
#Также определяется значение в этот момент другой точки
#Например - срабытывание защиты и первопричина, мощность  и частота в момент срабатывания

web=EdsWebApi()
web.connect()
web.logger.debug('lalalala')

ds=datetime.datetime(2020,11,1)
de=datetime.datetime(2020,12,30)


def getData(tag,tagVal):
	#tag - дискретная точка для анализа
	#tagVal - соответствующая точка, значение которой важно в момент срабатывания первой - в данном случае первопричина
	#tagP,tagF - мощность и частота в момент срабатывания        
	web.initOut(tag)
	print (tag)
	web.outString(tag,"{0};{1};".format(tag,tagVal))
	dt=ds
	data=[]
	while dt<de:	
		dt1=web.getDateChange(dt,de,tag,'F_INTOOVER_DT',[0.9])	#определяем следующий приход точки в 1
		if (dt1<dt):
			break
		val=web.getVal(dt1+datetime.timedelta(seconds=-3),tagVal) #определяем первопричину срабатывания СТОП
		record={'start':dt1, 'val':val}#формируем выходную строку
		data.append(record)
		web.outString(tag,'{start};{val};'.format(**record))#выводим строку в выходной файл
		print (dt1)
		if dt1<de:				
			dt2=web.getDateChange(dt1,de,tag,'F_INTOUNDER_DT',[0.1]) #определяем когда точка ушла обратно в 0
			if (dt2<dt1):
				break
			dt=dt2
		else:
			break


getData('05VT_VM03D-09.MCR@GRARM','05VT_LP01AO-03.MCR@GRARM')

