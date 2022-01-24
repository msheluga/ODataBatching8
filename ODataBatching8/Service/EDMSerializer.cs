using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ODataBatching8.Service
{
    public class EDMSerializer
    {
        public byte[] ConvertToBytes(IEdmModel model)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(memoryStream))
                {
                    IEnumerable<EdmError> errors;
                    CsdlWriter.TryWriteCsdl(model, writer, CsdlTarget.OData, out errors);
                }
                return memoryStream.ToArray();
            }
        }

        public IEdmModel ConvertFromBytes(byte[] modelBytes)
        {
            using (var ms = new MemoryStream(modelBytes))
            {
                using (var reader = XmlReader.Create(ms))
                {
                    IEnumerable<EdmError> errors;
                    IEdmModel model;
                    if (CsdlReader.TryParse(reader, out model, out errors))
                    {
                        return model;
                    }
                    throw new InvalidOperationException($"Model Error: {string.Join(",", errors.Select(_ => _.ErrorMessage))}");
                }
            }
        }
    }
}
