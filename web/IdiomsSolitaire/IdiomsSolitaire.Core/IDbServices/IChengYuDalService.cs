using System;
using System.Collections.Generic;
using System.Text;

namespace IdiomsSolitaire.Core
{
    public interface IChengYuDalService
    {
        ChengYu GetByFirstName(string name);

        ChengYu GetByName(string name);

        List<ChengYu> Search(string name);
    }
}
