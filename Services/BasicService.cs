using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services {
    public abstract class BasicService<T> : IService<T> where T : class, IEntity {
        protected abstract DbSet<T> DbSet { get; }
        protected abstract DbContext DbContext { get; }

        public T Get(int id) { return GetAll().Find(m => m.Id >= id); }

        public List<T> GetAll() { return DbSet.ToList(); }

        /**
         * <inheritdoc cref="IService{T}.Save" />
         * <returns>Het opgeslagen <see cref="Track">getrackde</see> entity</returns>
         * <seealso cref="Track" />
         * <seealso cref="SaveChanges" />
         */
        public T Save(T model) {
            T nModel = Add(model);
            SaveChanges();
            return nModel;
        }

        public void SaveAll(IEnumerable<T> models) {
            AddAll(models);
            SaveChanges();
        }

        /**
         * <inheritdoc cref="IService{T}.Update" />
         * <returns>Het aangepaste <see cref="Track">getrackde</see> entity</returns>
         * <seealso cref="Track" />
         * <seealso cref="SaveChanges" />
         */
        public T Update(T model) {
            model = DbSet.Update(model).Entity;
            SaveChanges();
            return model;
        }

        public T Delete(T model) {
            model = DbSet.Remove(model).Entity;
            SaveChanges();
            return model;
        }

        public void DeleteAll(IEnumerable<T> models) { DbSet.RemoveRange(models); }

        public int Count() { return DbSet.Count(); }

        /**
         * <summary>Tracked een model zodat je aanpassing er aan kan doen en vervolgens met <see cref="SaveChanges" /> kan opslaan</summary>
         * <returns>De getrackde model gebruik dit alleen als je zeker bent dat dit niet ergens wordt door gestuurd</returns>
         */
        public T Track(T model) {
            model = DbSet.Attach(model).Entity;
            return model;
        }

        /**
         * <returns>Het eerste element in de database</returns>
         */
        public T First() { return DbSet.First(); }

        /**
         * <summary>Slaat de wijzingen op van tracked models</summary>
         * <returns>De hoeveelheid aanpassing gemaakt</returns>
         * <seealso cref="Update" />
         * <seealso cref="Save" />
         * <seealso cref="Track" />
         */
        public int SaveChanges() {
            int i = DbContext.SaveChanges();
            return i;
        }

        public T Add(T model) {
            T nModel = DbSet.Add(model).Entity;
            return nModel;
        }

        public void AddAll(IEnumerable<T> models) { DbSet.AddRange(models); }
    }
}