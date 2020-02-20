from suds import client
import logging
import time
import datetime
import config
import logging
from logging import handlers
import os

#Класс на основе проекта интеграции АИИСКУЭ
class EdsWebApi():	
	def __init__(self):
		self.auth_str = ''
		self.soap_cln = None
		self.connected = False		
		self.logger = self.initLogging()
		self.outs=dict()
		self.out=self.initMainOut()

	def initLogging(self):
                #Инициализация логгирования в папку logs
		logger = logging.getLogger('eds')
		logger.setLevel("DEBUG")
		if not os.path.exists('logs'):
			os.makedirs('logs')
		log_file = handlers.RotatingFileHandler(filename='logs\eds.log',
			maxBytes=10000000,
			backupCount=10)
		#формат лога будет "дата сообщение"
		log_formater = logging.Formatter('%(asctime)s %(message)s')
		log_file.setFormatter(log_formater)
		logger.addHandler(log_file)
		#пишем тестовую строку в лог
		logger.debug('init')	
		return logger

	def initMainOut(self):
                #для упрощения вывода в текстовый файл в качестве выходного файла используется также файл лога но форматирование вида "сообщение"
                #имя файла будет вида out_[время_запуска_приложения]
                #ссылка на логгер является членом класса с именем out
		s='outEDS_{0}.txt'.format(time.time())
		logger = logging.getLogger(s)
		logger.setLevel("INFO")
		#все выходные файлы пишутся в папку out
		if not os.path.exists('out'):
			os.makedirs('out')
		log_file = handlers.WatchedFileHandler("out/{0}".format(s))
		log_formater = logging.Formatter('%(message)s')
		log_file.setFormatter(log_formater)
		logger.addHandler(log_file)	
		logger.debug('init')			
		return logger


	def initOut(self,name):
                #Если в одном приложении требуется несколько выходных файлов, они инициализируются с именем name
                #остальное аналогично mainOut
                #все ссылки на логгеры помещяются в словарь outs
		s='outEDS_{1}_{0}.txt'.format(time.time(),name)
		logger = logging.getLogger(s)
		logger.setLevel("INFO")
		if not os.path.exists('out'):
			os.makedirs('out')
		log_file = handlers.WatchedFileHandler("out/{0}".format(s))
		log_formater = logging.Formatter('%(message)s')
		log_file.setFormatter(log_formater)
		logger.addHandler(log_file)	
		logger.debug('init')	
		self.outs[name]=logger
		return logger
	
	def outString(self,name,s):
                #выводит строку в out файл с именем name
		out=self.outs[name]
		out.info(s)

	def connect(self):
                #Подключаемся к SOAP
		eds_wsdl = '%s://%s:%i/eds.wsdl' % (config.EDS_WEB_PROTOCOL,config.EDS_HOST,config.EDS_WEB_PORT)
		connected = False
		authorized = False
		try:
                        #получаем ссылку на клиента с настройками из файла config.py
			self.soap_cln = client.Client(eds_wsdl)
			self.logger.debug('[EDS_connector] Successfully connected to EDS')
			connected = True
		except Exception as conn_error:
			self.logger.debug('[EDS_connector] WSDL connection error to %s, %s' % (eds_wsdl, conn_error))
		if connected:
			try:
                                #авторизуемся на клиенте и получается ссылку на строку авторизации для работы с функциями
				self.auth_str = self.soap_cln.service.login(config.EDS_USER, config.EDS_PWD)
				self.logger.debug('[EDS_connector] Successfully authorized in EDS')
				authorized = True
			except Exception as auth_error:
				self.logger.debug('[EDS_connector] WebApi authorisation error with user %s and pass %s, %s' % (config.EDS_USER, config.EDS_PWD, auth_error))
		if connected and authorized:
				self.connected = True

	def eds_logout(self):
                #отключение от клиента
		try:
			self.soap_cln.service.logout(self.auth_str)
		except Exception as logour_error:
			self.logger.debug('[EDS_connector] Problem during EDS logout %s' % str(logour_error))
		finally:
			self.connected = False

	def runRequest(self,reqId):
                #Запуск запроса и ожидание завершения
		if reqId is not None:
                        #Ожидаем изменения состояние запроса на выполнено в течении TIMEOUT
			for i in range(1000 * config.TIMEOUT):
				try:
                                        #Узнаем статус запроса
					eds_request_status = self.soap_cln.service.getRequestStatus(self.auth_str, reqId)
				except Exception as request_error:
					self.logger.debug(u'[EDS_WebAPI_read]  EDS API problem: %s' % (str(request_error).encode('utf8')))						
					break
				self.logger.debug('[EDS_WebAPI_read] status: %s' % (str(eds_request_status.message)))
				if eds_request_status.status == 'REQUEST-SUCCESS':
					return True						
				if eds_request_status.status == 'REQUEST-FAILURE':
					return False					
				time.sleep(0.1)

	def createRequest(self,dateStart,dateEnd,period,items):
                #Создаем заготовку запроса с общими полями
		request = {
			'period':{
				'from':{
					'second':dateStart.timestamp()
					},
				'till':{
					'second':dateEnd.timestamp()
					}
				},
			'step':{
				'seconds':period
				},
			'items':items
		}
		return request

	def getDateChange(self,dateStart,dateEnd,tag,func,params):
                #Функция для определения следующего передаваемого тэга
                #получеает границы дат для анализа                
                #tag - точка для анализа
                #функция - н-р F_INTOOVER_DT
                #параметр необходимый для работы данной функции (см документацию)
		items=[]		
		item={
			'function':func,
			'pointId':{
				'iess':tag 
				},
			'params':params
			}
		items.append(item)#Формируем массив тэгов из одного элемента
		step=dateEnd.timestamp()-dateStart.timestamp()#шаг запроса будет равен всему интервалу 
		request = {
			'period':{
				'from':{
					'second':dateStart.timestamp()
					},
				'till':{
					'second':dateEnd.timestamp()
					}
				},
			'step':{
				'seconds':step
				},
			'items':items
		}
		#Запрашиваем табличный тренд, но т.к. шаг равен всему интервалу, можем получить только 1 строку результата
		reqId=self.soap_cln.service.requestTabular(self.auth_str,request)
		#Запускаем запрос
		self.runRequest(reqId)
		#Получаем ответ от сервера
		response=self.soap_cln.service.getTabular(self.auth_str,reqId)
		rows=[]
		rows=response['rows']
		if len(rows)>0:	#Если ответ от сервера содержит строки
                        #Нам нужна первая и единственная строка
			row=rows[0]			
			vals=row['values']
			if len(vals)>0:
                                #Нужен первый и единственный столбец
				try:
					#print("0")
					val=vals[0]['value'][0]
					#print(val)
					#print("1")
					dt1=datetime.datetime.fromtimestamp(val)#Получаем требуемую дату					
					#print(dt1)
					#print("2")
					
					#if dt1==dateStart:#Если дата не входит в требуемый интервал, значит функция не срабатывала, возвращаем конец интервала
					#return dt1						
					return dt1
				except Exception:
					#print("3")
					return dateEnd
			else:
				#print("1")
				return dateEnd
		else:
			#print("4")
			return dateEnd

	def getVal(self,dt,tag):
                #Функция возвращает значение указанного тэга в момент времени dt
		#print('{0}'.format(dt))
		items=[]
		item={
			'function':'VALUE',
			'pointId':{
				'iess':tag 
				}
			}
		items.append(item)#Формируем массив тэгов из одного элемента
		request = {
			'period':{
				'from':{
					'second':dt.timestamp()
					},
				'till':{
					'second':dt.timestamp()+10
					}
				},
			'step':{
				'seconds':1
				},
			'items':items
		}
		#Формируем запрос табличного тренда за 10 секунд с шагом 1 секунда. Нам нужен только результат 1 строки - это и есть требуемое значение
		reqId=self.soap_cln.service.requestTabular(self.auth_str,request)
		self.runRequest(reqId)
		
		response=self.soap_cln.service.getTabular(self.auth_str,reqId)
		rows=[]
		rows=response['rows']
		if len(rows)>0:#Разбираем первую строку для получения результата                        
			row=rows[0]
			ts=row['ts']['second']
			dt=datetime.datetime.fromtimestamp(ts)
			values=row['values']
			value=values[0]
			v=value['value'][0]
			return v
		return None


	def getDateChangeFull(self,dateStart,dateEnd,tag,valInit):
                #Функция для анализа любого изменения передаваемого тэга
                #Возвращает минимальную из двух дат - изменение значения вниз и вверх от valInit
		#print('{0}-{1} {2}'.format(dateStart,dateEnd,valInit))

		dt1=self.getDateChange(dateStart,dateEnd,tag,"F_INTOOVER_DT",[valInit+0.1])
		
		dt2=self.getDateChange(dateStart,dateEnd,tag,"F_INTOUNDER_DT",[valInit-0.1])		
		
		
		dt=dt1
		if dt2<dt1 :
			dt=dt2
		if dt==dateStart:
			dt=dt+datetime.timedelta(seconds=1)
		return dt

	def eds_qualityToLetter(self, eds_quality):
                #Преобразует качество точки в 1 букву
		return_quality = ''
		if eds_quality == 'QUALITY-BAD':
			return_quality = 'B'
		elif eds_quality == 'QUALITY-GOOD':
			return_quality = 'G'
		elif eds_quality == 'QUALITY-FAIR':
			return_quality = 'F'
		elif eds_quality == 'QUALITY-POOR':
			return_quality = 'P'
		return return_quality

	def getBitVal(self,val,bit):
                #Определяет значение бита bit в переданном значении val
		s=bin(val)
		if (len(s)-2)>bit:
			b=int(s[-bit-1])	
			return b==1
		return False

