import PyEds2
import time

live=PyEds2.createLive()
err=live.connect(True,False, {'rhost':'10.7.227.12','rport':43000,'lhost':'0.0.0.0','lport':0,'lport_range':32767})
print live.connInfo()

name='07VT_GC01A-16.MCR@GRARM'
try:
    live.waitTillSynced()
    print('sync')
    print live.connInfo()
except:
    print ('err')

live.subscribe(name)
print live.connInfo()
live.waitTillSynced()
tm=0
prevV=0
prevTS=0
while True:    
    tm=tm+1
    val=live.pointDynamicInfo(name)
    ts=val['TS'][0]
    v=val['AV']    
    if ((v!=prevV) | (ts-prevTS>0)):
        diff=ts-prevTS
        print(diff)
        prevTS=ts
        prevV=v
    time.sleep(0.3)
