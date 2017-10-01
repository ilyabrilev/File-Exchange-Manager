using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_Exchange_Manager
{
    public class FileSupertypeFake : FileSupertype
    {
        public bool isItWasAsource = false;
        public bool isItWasADest = false;

        public FileSupertypeFake(int _fileCount, int _bytesCount, WorkResult _res, bool _addAnError, ActionType _source_dest)
        {
            this.statistics.incrementBytes(_bytesCount);
            this.statistics.incrementFiles(_fileCount);            
            this.statistics.addResult(_res);
            if (_addAnError)
                this.statistics.addError("Test error");
            this.source_dest = _source_dest;
            
        }

        public override void ActionAsSource()
        {
            this.isItWasAsource = true;
        }

        public override void ActionAsDestination()
        {
            this.isItWasADest = true;
        }

        public override ConnectionType GetConnectionType()
        {
            return ConnectionType.Other;
        }
    }
}
