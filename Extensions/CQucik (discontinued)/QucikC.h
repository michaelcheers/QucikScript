union ObjectUnion
{
	double d;
	char* s;//string
};


struct Object
{
	ObjectType type;
	ObjectUnion data;
};

enum ObjectType
{
	Double,
	String
};

Object add (Object a, Object b)
{
	double aN = getNumber(a);
	double bN = getNumber(b);

	Object result;
	result.type = Double;
	result.data.d = aN + bN;
	return result;
}

Object subtract (Object a, Object b)
{
	double aN = getNumber(a);
	double bN = getNumber(b);

	Object result;
	result.type = Double;
	result.data.d = aN - bN;
	return result;
}

Object multiply(Object a, Object b)
{
	double aN = getNumber(a);
	double bN = getNumber(b);

	Object result;
	result.type = Double;
	result.data.d = aN * bN;
	return result;
}

Object divide(Object a, Object b)
{
	double aN = getNumber(a);
	double bN = getNumber(b);

	Object result;
	result.type = Double;
	result.data.d = aN / bN;
	return result;
}

double getNumber(Object value)
{
	if (value.type == Double)
		return value.data.d;
	else if (value.type == String)
	{
		double result;
		scanf("%lf", value.data.s, &result);
		return result;
	}
}