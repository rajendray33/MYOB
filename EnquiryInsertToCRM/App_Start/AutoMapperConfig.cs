using AutoMapper;
using EnquiryInsertToCRM.DataService;
using EnquiryInsertToCRM.Models;
using MYOB.AccountRight.SDK;
using MYOB.AccountRight.SDK.Contracts;

namespace EnquiryInsertToCRM
{
    public class AutoMapperConfig
    {
        public static void Initialize()
        {
            Mapper.Initialize((config) =>
            {
                config.CreateMap<AbEntryFieldInfo, AbEntryFieldInfoTemp>().ReverseMap();
                config.CreateMap<AbEntryFieldInfoTemp, AbEntryFieldInfo>().ReverseMap();
            });
        }
    }
}