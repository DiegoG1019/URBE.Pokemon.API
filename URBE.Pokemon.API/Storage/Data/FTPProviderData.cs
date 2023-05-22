using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URBE.Pokemon.API.Storage.Data;
public readonly record struct FTPProviderData(string Host, ushort Port, string? Username, string? Password, bool ValidateAnyCertificate = false);
