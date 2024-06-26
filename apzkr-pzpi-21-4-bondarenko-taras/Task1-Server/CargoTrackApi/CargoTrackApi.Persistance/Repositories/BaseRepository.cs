﻿using CargoTrackApi.Domain.Common;
using CargoTrackApi.Persistance.Database;
using CargoTrackApi.Application.IRepositories;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CargoTrackApi.Persistance.Repositories
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : EntityBase
    {
        protected MongoDbContext _db;

        protected IMongoCollection<TEntity> _collection;

        public BaseRepository(MongoDbContext db, string collectionName)
        {
            this._db = db;
            this._collection = _db.Db.GetCollection<TEntity>(collectionName);
        }

        public async Task<TEntity> GetOneAsync(ObjectId id, CancellationToken cancellationToken)
        {
            return await this._collection
                .Find(e => e.Id == id && !e.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TEntity> GetOneAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            return await this._collection.Find(predicate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            return await this._collection
                .FindAsync(predicate).Result.ToListAsync();
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await this._collection.InsertOneAsync(entity, new InsertOneOptions(), cancellationToken);
            return entity;
        }

        public async Task<List<TEntity>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await this._collection.Find(e => !e.IsDeleted)
                                         .Skip((pageNumber - 1) * pageSize)
                                         .Limit(pageSize)
                                         .ToListAsync(cancellationToken);
        }

        public async Task<List<TEntity>> GetPageAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            return await this._collection.Find(predicate)
                                         .Skip((pageNumber - 1) * pageSize)
                                         .Limit(pageSize)
                                         .ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
        {
            return (int)await this._collection.CountDocumentsAsync(e => !e.IsDeleted, cancellationToken: cancellationToken);
        }

        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            return (int)await this._collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            return await this._collection.Find(predicate).AnyAsync(cancellationToken);
        }

        public async Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            var updateDefinition = Builders<TEntity>.Update
                .Set(e => e.IsDeleted, true)
                .Set(e => e.LastModifiedById, entity.LastModifiedById)
                .Set(e => e.LastModifiedDateUtc, entity.LastModifiedDateUtc);

            var options = new FindOneAndUpdateOptions<TEntity>
            {
                ReturnDocument = ReturnDocument.After
            };

            return await this._collection.FindOneAndUpdateAsync(
                Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id), updateDefinition, options, cancellationToken);
        }
        public async Task<TEntity> DeleteFromDbAsync(TEntity entity, CancellationToken cancellationToken)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", entity.Id);
            var result = await _collection.DeleteOneAsync(filter, cancellationToken);

            return result.DeletedCount > 0 ? entity : null;
        }
    }
}
