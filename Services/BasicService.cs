using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services {
    public class BasicService<T> : IService<T> where T : class, IEntity {
        private DbSet<T> _dbSet;
        private DbContext _dbContext;

        public BasicService(DbSet<T> dbSet, DbContext dbContext) {
            _dbSet = dbSet;
            _dbContext = dbContext;
        }

        public T Get(int id) { return _dbSet.Find(id); }

        public List<T> GetAll() { return _dbSet.ToList(); }

        /**
         * <inheritdoc cref="IService{T}.Save"/>
         * <returns>Het opgeslagen <see cref="Track">getrackde</see> entity</returns>
         * <seealso cref="Track"/>
         * <seealso cref="SaveChanges"/>
         */
        public T Save(T model) {
            model = _dbSet.Add(model).Entity;
            _dbContext.SaveChanges();
            return model;
        }

        public void SaveAll(IEnumerable<T> models) {
            _dbSet.AddRange(models);
            _dbContext.SaveChanges();
        }
        
        /**
         * <inheritdoc cref="IService{T}.Update"/>
         * <returns>Het aangepaste <see cref="Track">getrackde</see> entity</returns>
         * <seealso cref="Track"/>
         * <seealso cref="SaveChanges"/>
         */
        public T Update(T model) {
            model = _dbSet.Update(model).Entity;
            _dbContext.SaveChanges();
            return model;
        }

        public T Delete(T model) { 
            model = _dbSet.Remove(model).Entity;
            _dbContext.SaveChanges();
            return model;
        }

        public void DeleteAll(IEnumerable<T> models) {
            _dbSet.RemoveRange(models);
        }
        
        public int Count() {
            return _dbSet.Count();
        }

        /**
         * <summary>Tracked een model zodat je aanpassing er aan kan doen en vervolgens met <see cref="SaveChanges"/> kan opslaan</summary>
         * <returns>De getrackde model gebruik dit alleen als je zeker bent dat dit niet ergens wordt door gestuurd</returns>
         */
        public T Track(T model) {
            model = _dbSet.Attach(model).Entity;
            return model;
        }

        /**
         * <summary>Slaat de wijzingen op van tracked models</summary>
         * <returns>De hoeveelheid aanpassing gemaakt</returns>
         * <seealso cref="Update"/>
         * <seealso cref="Save"/>
         * <seealso cref="Track"/>
         */
        public int SaveChanges() {
            return _dbContext.SaveChanges();
        }
    }
}