using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    interface Token
    {
        void SetId(int id);
        int GetId();
        void SetPosition(int position);
        int GetPosition();
        void SetInFinalList(bool inFinal);
        bool InFinalList();
    }
}
