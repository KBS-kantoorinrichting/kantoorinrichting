using System.Collections.Generic;
using Models;

namespace Services {
    public interface IService<T> where T : IEntity {
     /**
         * <param name="id">Het id van de entity wat je wilt.</param>
         * <returns>De entity met het <paramref name="id" /> van type <typeparamref name="T" />.</returns>
         */
     T Get(int id);

     /**
         * <returns>Alle entities die in de database staan van type <typeparamref name="T" />.</returns>
         */
     List<T> GetAll();

     /**
         * <summary>Slaat de entity op en geeft deze terug.</summary>
         * <param name="model">De entity wat opgeslagen wordt.</param>
         * <returns>De opgeslagen entity.</returns>
         */
     T Save(T model);

     /**
         * <summary>Slaat alle entities op uit <paramref name="models" />.</summary>
         * <param name="models">De entities die opgeslagen moeten worden.</param>
         */
     void SaveAll(IEnumerable<T> models);

     /**
         * <summary>Past de entity aan en geef deze terug.</summary>
         * <param name="model">De entity wat aangepast wordt.</param>
         * <returns>De aangepaste entity.</returns>
         */
     T Update(T model);

     /**
         * <summary>Verwijderd de entity aan en geef deze terug.</summary>
         * <param name="model">De entity wat verwijderd wordt.</param>
         * <returns>De verwijderd entity.</returns>
         */
     T Delete(T model);

     /**
         * <summary>Verwijderd alle entities uit <paramref name="models" />.</summary>
         * <param name="models">De entities die verwijderd moeten worden.</param>
         */
     void DeleteAll(IEnumerable<T> models);

        /**
         * <returns>Het aantal entities dat aanwezig zijn.</returns>
         */
        int Count();
    }
}