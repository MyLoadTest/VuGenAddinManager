﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;

namespace MyLoadTest.VuGenAddInManager.Model
{
    public abstract class Model<TModel> : INotifyPropertyChanged
    {
        private IAddInManagerServices _services;

        public event PropertyChangedEventHandler PropertyChanged;

        protected Model()
        {
            if (WpfHelper.IsInDesignMode())
            {
                return;
            }

            // Use default services container
            _services = AddInManagerServices.Services;
        }

        protected Model(IAddInManagerServices services)
        {
            _services = services;
        }

        public string PropertyChangedFor<TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            return PropertyChangedFor(memberExpression);
        }

        protected void OnPropertyChanged<TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            var propertyName = PropertyChangedFor(expression);
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected IAddInManagerServices AddInManager
        {
            get
            {
                return _services;
            }

            set
            {
                _services = value;
            }
        }

        private static string PropertyChangedFor(MemberExpression memberExpression)
        {
            if (memberExpression != null)
            {
                return memberExpression.Member.Name;
            }

            return string.Empty;
        }
    }
}