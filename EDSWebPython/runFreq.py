import config
import datetime
import time
from webAPI import EdsWebApi



web=EdsWebApi()
web.connect()
web.logger.debug('lalalala')

ds=datetime.datetime(2018,10,1,0,0)
de=datetime.datetime(2018,10,23,0,0)
#tags={'07VT_LP01AO-03.MCR@GRARM','07VT_TW00AI-05.MCR@GRARM','04VT_AM01P-47.MCR@GRARM' }
tagF='07VT_EX01SD-15.MCR@GRARM'
tagS='07VT_AM03P-01.MCR@GRARM'
bit=14

reqId=None
items=[]

item={
	'function':'AVG',
	'pointId':{
		'iess':tagF 
		}
	}
items.append(item)
item={
	'function':'MIN_VALUE',
	'pointId':{
		'iess':tagF 
		}
	}
items.append(item)
item={
	'function':'MAX_VALUE',
	'pointId':{
		'iess':tagF 
		}
	}
items.append(item)



step=60

web.initOut("0_1")
web.initOut("0_05")
web.initOut("0_2")


data=[]
val=web.getVal(ds,tagS)
bitVal=web.getBitVal(val,bit)
onDT=None
offDT=None
if bitVal:
	onDT=ds
dt=ds
while dt<de:	
	d1=web.getDateChangeFull(dt,de,tagS,val)
	valNew=web.getVal(d1,tagS)
	bitValNew=web.getBitVal(valNew,bit)
	if val!=valNew:
		val=valNew
		dt=d1
		if bitVal != bitValNew:
			bitVal=bitValNew
			if bitVal:
				onDT=d1
			else:
				offDT=d1
				record={'start':onDT, 'stop':offDT}
				data.append(record)
				print('{0} - {1} [{2}]'.format(onDT,offDT,offDT.timestamp()-onDT.timestamp()))
	else:
		dt=d1

for record in data:	
	dt=record['start']
	de=record['stop']	
	dt=dt+datetime.timedelta(seconds=60)
	de=de+datetime.timedelta(seconds=-60)
	print("Анализ {0}-{1}".format(dt,de))

	while dt<de:
		de1=dt+datetime.timedelta(hours=12)
		if (de1>de):
			de1=de
		print("{0}".format(dt))

		request=web.createRequest(dt,de1,step,items)
		reqId=web.soap_cln.service.requestTabular(web.auth_str,request)
		web.runRequest(reqId)

		response=web.soap_cln.service.getTabular(web.auth_str,reqId)

		rows=response['rows']
		for row in rows:
			ts=row['ts']['second']
			d1=datetime.datetime.fromtimestamp(ts)		
			values=row['values']

			vs=[]
			vs.append(values[0]['value'][0])		
			vs.append(values[1]['value'][0])		
			vs.append(values[2]['value'][0])		
			vs.sort()
			vMin=vs[0]
			vAvg=vs[1]
			vMax=vs[2]			
			
			s='{0};{1};{2};{3}'.format(d1,vAvg,vMin,vMax)
			if (abs(vAvg-vMin)>0.1) | (abs(vAvg-vMax)>0.1) | (abs(vMin-vMax)>0.1) | (abs(vMin-50)>0.1) | (abs(vMax-50)>0.1) :				
				web.outString("0_1",s)
			if (abs(vAvg-vMin)>0.2) | (abs(vAvg-vMax)>0.2) | (abs(vMin-vMax)>0.2) | (abs(vMin-50)>0.2) | (abs(vMax-50)>0.2):
				web.outString("0_2",s)
			if (abs(vAvg-vMin)>0.05) | (abs(vAvg-vMax)>0.05) | (abs(vMin-vMax)>0.05) | (abs(vMin-50)>0.05) | (abs(vMax-50)>0.05) :
				web.outString("0_05",s)
		dt=dt+datetime.timedelta(hours=12)
		if (dt>=de):
			break