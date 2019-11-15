using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace SchoolManagement.Common
{
    public abstract class BaseEntity<TKey> : IBaseEntity<TKey>
    {
        [NotMapped]
        object IBaseEntity.Id => this.Id;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual TKey Id { get; set; }
        public string CreateUserId { get; private set; }
        public DateTime? CreateTime { get; private set; }
        public string EditUserId { get; private set; }
        public DateTime? EditTime { get; private set; }
        [JsonIgnore]
        public bool IsDeleted { get; set; }
        [JsonIgnore]
        public byte[] Timestamp { get; set; }
        [NotMapped]
        [JsonIgnore]
        public bool IsNew => this.Id.Equals(default(TKey));

        public virtual void Map(IMapper mapper, BaseEntity<TKey> dest)
        {
            mapper.Map(this, dest);
        }
    }

    public interface IBaseEntity<TKey> : IBaseEntity
    {
        new TKey Id { get; set; }
    }

    public interface IBaseEntity
    {
        object Id { get; }
        string CreateUserId { get; }
        DateTime? CreateTime { get; }
        string EditUserId { get; }
        DateTime? EditTime { get; }
        bool IsDeleted { get; set; }
        byte[] Timestamp { get; set; }
        bool IsNew { get; }
    }
}
