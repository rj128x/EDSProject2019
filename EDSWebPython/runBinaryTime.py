import config
import datetime
import time
from webAPI import EdsWebApi

#Сценарий для получения всех периодов когда точка была сработана (с опредлением длительности)

web=EdsWebApi()
web.connect()
web.logger.debug('lalalala')

ds=datetime.datetime(2020,1,18)
de=datetime.datetime(2020,2,17)
#tag='25VT_EA5010DM-103.UNIT0@SCADA'


def getData(tag,tagVal):
        print (tag)
        web.initOut(tag)
        dt=ds
        dt1=ds
        dt2=ds
        data=[]
        #определяем значение точки в начале интервала)
        valStart=web.getVal(ds,tag)
        web.outString(tag,'Включение;Отключение;Работа;уровень')
        print(valStart)
        if (valStart>0):#Если точка сработана, то определяем сначала, когда она стала 0
                dt1=ds
                dt2=web.getDateChange(dt1,de,tag,'F_INTOUNDER_DT',[0.5])
                ln=dt2.timestamp()-dt1.timestamp()
                ln=ln#длительность работы в часах
                record={'start':dt1,'end':dt2,'len':ln}
                data.append(record)
                if ln>0:
                        web.outString(tag,'{start};{end};{len:.3f}'.format(**record))#Выводим строку в файл
                dt=dt2        
        while dt<de:#Дальше в цикле определяем все сработки точки аналогично п 1
          dt1=web.getDateChange(dt,de,tag,'F_INTOOVER_DT',[0.5])
          #val1=web.getVal(dt1,tagVal)
          if dt1<de:
            ln2=dt1.timestamp()-dt2.timestamp()
            dt2=web.getDateChange(dt1,de,tag,'F_INTOUNDER_DT',[0.5])
            #val2=web.getVal(dt2,tagVal)
            ln=dt2.timestamp()-dt1.timestamp()
            ln=ln
            record={'start':dt1,'end':dt2,'len2':ln2,'len':ln}
            data.append(record)
            if ln>0:
                web.outString(tag,'{start};{end};{len:.0f};'.format(**record))#Выводим строку в файл   
            dt=dt2+datetime.timedelta(seconds=1)
          else:
            break

getData('53VT_MU5300DM-201.UNIT0@SCADA','')
