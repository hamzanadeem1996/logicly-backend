// using Apex.DataAccess.Models;
// using NPoco;
//
// namespace Apex.DataAccess.Repositories
// {
//     public class VisitLockStatusRepo
//     {
//         public VisitLockStatus Get(int PatientId)
//         {
//             var cmd = Sql.Builder.Select("*").From("VisitLockStatus");
//             cmd.Where("PatientId=@0", PatientId);
//             using (var db = Utility.Database)
//             {
//                 var result = db.FirstOrDefault<VisitLockStatus>(cmd);
//                 return result;
//             }
//         }
//
//         public object Save(VisitLockStatus visitLock)
//         {
//             using (var db = Utility.Database)
//             {
//                 db.Save(visitLock);
//                 return visitLock;
//             }
//         }
//     }
// }