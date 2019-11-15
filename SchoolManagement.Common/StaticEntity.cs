using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SchoolManagement.Common
{
    public class StaticEntity : BaseEntity<int>, IStaticEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override int Id { get; set; }
    }

    public interface IStaticEntity : IBaseEntity<int>
    {

    }
}
