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

ds=datetime.datetime(2018,9,1)
de=datetime.datetime(2018,11,27)


def getData(tag,tagVal,tagP,tagF):
        #tag - дискретная точка для анализа
        #tagVal - соответствующая точка, значение которой важно в момент срабатывания первой - в данном случае первопричина
        #tagP,tagF - мощность и частота в момент срабатывания        
        
	print (tag)
	web.out.info("{0};{1};{2};{3}".format(tag,tagVal,tagP,tagF))
	dt=ds
	data=[]
	while dt<de:	
		dt1=web.getDateChange(dt,de,tag,'F_INTOOVER_DT',[0.9])	#определяем следующий приход точки в 1
		if dt1<de:				
			dt2=web.getDateChange(dt1,de,tag,'F_INTOUNDER_DT',[0.1]) #определяем когда точка ушла обратно в 0
			val=web.getVal(dt1+datetime.timedelta(seconds=10),tagVal) #определяем первопричину срабатывания СТОП
			valP=web.getVal(dt1+datetime.timedelta(seconds=-60),tagP)#мощность за 60 секунд до сработки
			valF=web.getVal(dt1+datetime.timedelta(seconds=-60),tagF)#частота за 60 секунд до сработки
			len=dt2.timestamp()-dt1.timestamp()#определяем длительность, сколько "висед" сигнал аварии
			record={'start':dt1,'end':dt2,'len':len, 'val':val, 'valP':valP,'valF':valF}#формируем выходную строку
			data.append(record)
			#web.out.info ('{start} - {end}: {len}'.format(**record))
			web.out.info ('{start};{val};{valP};{valF}'.format(**record))#выводим строку в выходной файл
			dt=dt2
		else:
			break

getData('04VT_AC31D-22.MCR@GRARM','04VT_AC31A-01.MCR@GRARM','04VT_LP01AO-03.MCR@GRARM','20VT_CE7041AM-120.UNIT0@SCADA')
getData('04VT_AC30D-22.MCR@GRARM','04VT_AC30A-01.MCR@GRARM','04VT_LP01AO-03.MCR@GRARM','20VT_CE7041AM-120.UNIT0@SCADA')

getData('07VT_AC31D-22.MCR@GRARM','07VT_AC31A-01.MCR@GRARM','07VT_LP01AO-03.MCR@GRARM','20VT_CE7071AM-120.UNIT0@SCADA')
getData('07VT_AC30D-22.MCR@GRARM','07VT_AC30A-01.MCR@GRARM','07VT_LP01AO-03.MCR@GRARM','20VT_CE7071AM-120.UNIT0@SCADA')

