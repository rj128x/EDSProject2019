import config
import datetime
import time
from webAPI import EdsWebApi


#Простой сценарий для запуска получения аггрегированных данных массива точек с заданным шагом
#Если диапазон очень большой может иметь смысл раздробить интервал на подзапросы и выполнить их в цикле
web=EdsWebApi()
web.connect()
web.logger.debug('lalalala')

ds=datetime.datetime(2019,4,25,10,21)
de=datetime.datetime(2019,4,25,10,23)

#Массив точек для анализа
tags={'02VT_TC03A-011.UNIT01@BLOCK1','07VT_TC01A-06.MCR@GRARM'}
#tags={'61VT_UB16D-206.UNIT0@SCADA' }

#ID запроса
reqId=None
items=[]
for tag in tags:
	item={
		'function':'VALUE', #Функция аггрегации (минимум, максимум, среднее) см документацию
		'pointId':{
			'iess':tag 
			}
		}
	items.append(item)

request=web.createRequest(ds,de,1,items)#Здесь создаем запрос с нужным шагом

reqId=web.soap_cln.service.requestTabular(web.auth_str,request)#Запускаем запрос
web.runRequest(reqId)

response=web.soap_cln.service.getTabular(web.auth_str,reqId)#Получаем ответ

points=response['pointsIds']
for point in points:
	print(point)

rows=response['rows']
for row in rows:#Разбираем полученные строки
	ts=row['ts']['second']
	dt=datetime.datetime.fromtimestamp(ts)#Дата строки
	print(dt)
	values=row['values']
	#Получаем все значения. порядок в массиве соответствует порядку в массиве points и tags
	#Здесь может быть дополнительная обработка полученных значений
	vs=[]
	for value in values:
		v=value['value'][0]
		vs.append('{}'.format(v))
	print(vs)
	s='{0};{1}'.format(dt,';'.join(vs))#получаем строку для вывода в файл
	web.out.info(s)#выводим значение в файл
