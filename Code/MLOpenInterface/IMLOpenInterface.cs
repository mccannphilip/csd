using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace MLOpenInterface
{
    [ServiceContract]
    public interface IMLOpenInterface
    {
        /// <summary>
        /// Input array:
        ///     [0]: From address
        ///     [1]: To address
        ///     [2]: Subject
        ///     [3]: Plain Text Body
        /// Output array:
        ///     [0]: Skillset
        ///     [1]: Priority
        ///     [2]: Auto-Close
        /// </summary>
        /// <param name="inputParameters"></param>
        /// <returns></returns>
        [OperationContract]
        string[] MLRouting(string[] inputParameters);
    }
}
