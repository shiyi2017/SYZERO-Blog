﻿using System;
using System.Collections.Generic;
using System.Text;
using SyZero.Domain.Model;

namespace SyZero.Domain.Repository
{
    public interface IRepository<TEntity> : IBaseRepository<TEntity, string> where TEntity : class, IEntity
    {
      
    }
}
