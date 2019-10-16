import config
import datetime
import time
from webAPI import EdsWebApi


#Простой сценарий для запуска получения аггрегированных данных массива точек с заданным шагом
#Если диапазон очень большой может иметь смысл раздробить интервал на подзапросы и выполнить их в цикле
web=EdsWebApi()
web.connect()
web.logger.debug('lalalala')

ds=datetime.datetime(2019,4,1,0,0)
de=datetime.datetime(2019,5,1,0,0)

#Массив точек для анализа
tags={'11VT_GP00AP-125.MCR@GRARM','10VT_GC00A-105.UNIT09@BLOCK5'}
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

dt=ds
index=0
PrevVals=[]     
for i in range(10):        
        PrevVals.append(0)
inited=False
while dt<de:
        print(dt)
        request=web.createRequest(dt,dt+datetime.timedelta(hours=1),1,items)#Здесь создаем запрос с нужным шагом

        reqId=web.soap_cln.service.requestTabular(web.auth_str,request)#Запускаем запрос
        web.runRequest(reqId)

        response=web.soap_cln.service.getTabular(web.auth_str,reqId)#Получаем ответ

        #points=response['pointsIds']
        #for point in points:
        #        print(point)

        rows=response['rows']
        for row in rows:#Разбираем полученные строки
                ts=row['ts']['second']
                dt1=datetime.datetime.fromtimestamp(ts)#Дата строки
                #print(dt)
                values=row['values']
                #Получаем все значения. порядок в массиве соответствует порядку в массиве points и tags
                #Здесь может быть дополнительная обработка полученных значений
                vs=[]
                vals=[]
                for value in values:
                        v=value['value'][0]
                        vs.append('{}'.format(v))
                        vals.append(v)
                f=vals[0]
                p=vals[1]

                if inited:
                        prevInd=10-index-1
                        prevP=PrevVals[prevInd]
                        diffP=prevP-p
                        if diffP>0.4:
                                if ((f>49.925) & (f<49.98)):
                                        print("{0}: {1} {2} {3}".format(dt1,prevP,p,f))
                        if diffP<-0.4:
                                if ((f>50.02)&(f<50.075)):
                                        print("{0}: {1} {2} {3}".format(dt1,prevP,p,f))

                PrevVals[index]=p
                index=index+1
                if index==10:
                        index=0
                        inited=True                                        
                
                #print(vs)
                s='{0};{1}'.format(dt,';'.join(vs))#получаем строку для вывода в файл
                web.out.info(s)#выводим значение в файл
        dt=dt+datetime.timedelta(hours=1)
