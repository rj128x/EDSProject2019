
import pyxeds2


import time
print "live"

lv=pyxeds2.createLive()
print lv
err=lv.init(pyxeds2.LiveMode.ReadWrite,'0.0.0.0',0,'10.7.227.12',43000,0,32767)
print err
print pyxeds2.liveErrMessage(err)

print "arch"

arch=pyxeds2.createArch()
err=arch.init('0.0.0.0',0,'10.7.227.12',43001,0,50)
print pyxeds2.archErrMessage(err)



t=long(time.time())
print t


pt=lv.findByIESS('07VT_GC02D-45.MCR@GRARM')
print pt
sid=lv.getSID(pt);
print sid

f=arch.getFunction('OVER_TIME')
f.pushPointParam(sid,0xff,pyxeds2.ArchShadeMode.PreferArch)
f.pushValueParam(0.1);
f.pushTimestampParam(t-3600*24*20)
f.pushTimestampParam(t-3600*24*0)
fID=arch.addQuery(f)
print fID

arch.executeQueries()
try:

    result=arch.getResponse(fID)
    print result
except:
    print 'error'

arch.clear()

f=arch.getFunction('UNDER_TIME')
f.pushPointParam(sid,0xff,pyxeds2.ArchShadeMode.PreferArch)
f.pushValueParam(0.9);
f.pushTimestampParam(t-3600*24*20)
f.pushTimestampParam(t-3600*24*0)
fID=arch.addQuery(f)
print fID
arch.executeQueries()
try:

    result=arch.getResponse(fID)
    print result
except:
    print 'error'

