using System.ComponentModel.DataAnnotations.Schema;

namespace Models {
    public interface IEntity {
        /**
         * De primary key van de entity
         */
        public int Id { get; set; }
    }
}