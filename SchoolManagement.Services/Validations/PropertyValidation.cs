using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Common;
using SchoolManagement.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Services.Validations
{
    public class PropertyValidation<TEntity, TProperty> : IPropertyValidation<TEntity>
         where TEntity : class, IBaseEntity, new()
    {
        private IServiceProvider _serviceProvider;
        private readonly IEFRepository<TEntity> _repository;
        private readonly Expression<Func<TEntity, TProperty>> _property;
        private readonly ValidationType _type;
        private readonly string _caption;
        private Func<TEntity, string> _errorExpression;
        private IEFRepository _foreignKeyRepository;
        private Expression<Func<TEntity, bool>> _validationExpression;
        private Func<TEntity, bool> _customExpression;
        private Func<TEntity, Task<bool>> _customExpressionAsync;
        private PropertyValidation<TEntity, TProperty> _nextPropertyValidation;
        internal PropertyValidation(IServiceProvider serviceProvider, IEFRepository<TEntity> repository, Expression<Func<TEntity, TProperty>> property, ValidationType validationType, string caption)
        {
            _serviceProvider = serviceProvider;
            _repository = repository;
            _property = property;
            _type = validationType;
            _caption = caption;
        }

        private PropertyValidation<TEntity, TProperty> NextValidation(ValidationType validationType) => _nextPropertyValidation = new PropertyValidation<TEntity, TProperty>(_serviceProvider, _repository, _property, validationType, _caption);
        public PropertyValidation<TEntity, TProperty> Mandatory()
        {
            return NextValidation(ValidationType.Mandatory);
        }
        public PropertyValidation<TEntity, TProperty> Mandatory(Expression<Func<TEntity, bool>> validationCondition, Func<TEntity, string> errorExpression = null)
        {
            NextValidation(ValidationType.Mandatory);
            _nextPropertyValidation._validationExpression = validationCondition;
            _nextPropertyValidation._errorExpression = errorExpression;
            return _nextPropertyValidation;
        }
        public PropertyValidation<TEntity, TProperty> Duplicate(Func<TEntity, string> errorExpression = null)
        {
            NextValidation(ValidationType.Duplicate);
            _nextPropertyValidation._errorExpression = errorExpression;
            return _nextPropertyValidation;
        }
        public PropertyValidation<TEntity, TProperty> Custom(Func<TEntity, bool> expression, Func<TEntity, string> errorExpression)
        {
            NextValidation(ValidationType.Expression);
            _nextPropertyValidation._customExpression = expression;
            _nextPropertyValidation._errorExpression = errorExpression;
            return _nextPropertyValidation;
        }
        public PropertyValidation<TEntity, TProperty> Custom(Func<TEntity, Task<bool>> expression, Func<TEntity, string> errorExpression)
        {
            NextValidation(ValidationType.Expression);
            _nextPropertyValidation._customExpressionAsync = expression;
            _nextPropertyValidation._errorExpression = errorExpression;
            return _nextPropertyValidation;
        }

        public PropertyValidation<TEntity, TProperty> ForeignKey<TRepository>(Func<TEntity, string> errorExpression = null)
            where TRepository : IEFRepository
        {
            NextValidation(ValidationType.ForeignKey);
            _nextPropertyValidation._foreignKeyRepository = _serviceProvider.GetRequiredService<TRepository>();
            _nextPropertyValidation._errorExpression = errorExpression;
            return _nextPropertyValidation;
        }

        public async Task<(bool valid, string error)> Validate(TEntity entity)
        {
            (bool, string) result = (true, string.Empty);
            if (_nextPropertyValidation == null)
            {
                return result;
            }

            var propertyFunc = _property.Compile();
            var value = propertyFunc(entity);

            switch (_type)
            {
                case ValidationType.None:
                    result = await _nextPropertyValidation.Validate(entity);
                    break;
                case ValidationType.Mandatory:
                    if (!EqualityComparer<TProperty>.Default.Equals(value, default(TProperty)))
                    {
                        result = await _nextPropertyValidation.Validate(entity);
                    }
                    else
                    {
                        result = (false, _errorExpression?.Invoke(entity) ?? $"{_caption} is required.");
                    }
                    break;
                case ValidationType.Duplicate:
                    var conditions = new[] { _validationExpression, o => propertyFunc(o).Equals(value) && o.Id != entity.Id };

                    if (await _repository.Any(conditions))
                    {
                        result = await _nextPropertyValidation.Validate(entity);
                    }
                    else
                    {
                        result = (false, _errorExpression?.Invoke(entity) ?? $"{_caption} can not be duplicate.");
                    }
                    break;
                case ValidationType.Expression:
                    if (_customExpression(entity))
                    {
                        result = await _nextPropertyValidation.Validate(entity);
                    }
                    else
                    {
                        result = (false, _errorExpression(entity));
                    }
                    break;
                case ValidationType.ExpressionAync:
                    if (await _customExpressionAsync(entity))
                    {
                        result = await _nextPropertyValidation.Validate(entity);
                    }
                    else
                    {
                        result = (false, _errorExpression(entity));
                    }
                    break;
                case ValidationType.ForeignKey:
                    if (await _foreignKeyRepository.Any(value))
                    {
                        result = await _nextPropertyValidation.Validate(entity);
                    }
                    else
                    {
                        result = (false, _errorExpression?.Invoke(entity) ?? $"Foreign key for {_caption} does not exist.");
                    }
                    break;
            }
            return result;
        }
    }

    public interface IPropertyValidation<TEntity>
    {
        Task<(bool valid, string error)> Validate(TEntity entity);
    }

    enum ValidationType
    {
        None,
        Mandatory,
        Duplicate,
        Expression,
        ExpressionAync,
        ForeignKey,
    }
}
