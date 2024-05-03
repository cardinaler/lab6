// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"
#include <time.h>
#include <mkl.h>
#include <iostream>
#include <string>
using namespace std;

/*
Суть работы минимазации(объяснение для себя).
Имеется функция (обертка над CubicSpline), которой на вход поступают значения Y = (y_1, ... , y_m) в заранее выбранных узлах
равномерной сетки (число узлов = m).
На выход она возращает вектор, поля которого это значения невязки F = (F1, F2, ..., Fn) на сетке (число узлов = n == nS)
между вычисленными значениями в этих узлахи истинными значения y_true.
Необходимо подобрать оптимальный вход (y_1, ... , y_m), чтобы минимизировать Евклидову норму вектора F
*/
enum class ErrorEnum { NO, INIT_Err, CHECK_Err, SOLVE_Err, JACOBI_Err, GET_Err, DELETE_Err, RCI_Err };


void CubicSpline(
	int nX,					// число узлов сплайна == m (для построения)
	double* X,				// массив узлов сплайна (для построения)
	int nY,					// размерность векторной функции (для построения)
	double* Y,				// массив заданных значений векторной функции (для построения) (Этот вход и нужно сделать оптимальным)
	int nS,					// число узлов сетки, на которой вычисляются значения сплайна (для расчета)
	double* grid,			// сетка (для расчета)
	double* splineValues,	// массив вычисленных значений сплайна на сетке grid
	double* y_true,         // Истинные значения функции на сетке grid
	double* F,				// Расчет невязки (стремимся к ее минимуму)
	bool DebugMode = false)	// Режим отладки		
{
	
	MKL_INT order = DF_PP_CUBIC;      //кубический сплайн
	MKL_INT type = DF_PP_NATURAL;     //классичейский сплайн
	MKL_INT bc_type = DF_BC_FREE_END; //Условие свободного конца
	double* scoeff = new double[nY * (nX - 1) * order];
	try
	{
		
		DFTaskPtr my_task;
		int status = -1;
		status = dfdNewTask1D(&my_task, nX, X, DF_UNIFORM_PARTITION, nY, Y, DF_NO_HINT);
		if (status != DF_STATUS_OK)
		{
			throw "Error in dfdNewTask1D";
		}
		
		double bc[2]{ 0,   // Вторая производная сплайна на левом конце
					  0 }; // Вторая производная сплайна на правом конце
		
		status = dfdEditPPSpline1D(my_task, order, type, bc_type, bc, DF_NO_IC, NULL, scoeff, DF_NO_HINT);
		
		scoeff; // Это коэффициенты сплайна 4(n-1) штук

		if (status != DF_STATUS_OK)
		{
			throw "Error in dfdEditPPSpline1D";
		}

		status = dfdConstruct1D(my_task, DF_PP_SPLINE, DF_METHOD_STD);
		if (status != DF_STATUS_OK)
		{
			throw "Error in dfdConstruct1D";
		}

		int nDorder = 1; // число производных, которые вычисляются, плюс 1
		MKL_INT dorder[] = { 1 };

		if (DebugMode)   // Режим отладки
		{
			setlocale(LC_ALL, "");
			cout << "/////////////////////Отладочная информация//////////////////" << endl;
			nDorder = 3; //Вычисление до второй производной
			MKL_INT dorder[] = {1, 1, 1}; //Вычисление значений и второй производной
			double* DebugRes = new double[3 * nX]; 
			status = dfdInterpolate1D(my_task, DF_INTERP, DF_METHOD_PP, nX, X, DF_UNIFORM_PARTITION, nDorder,
				dorder, NULL, DebugRes, DF_NO_HINT, NULL);
			if (status != DF_STATUS_OK)
			{
				throw "Error in dfdInterpolate1D";
			}
			cout << "Значения в узлах интерполяции" << endl;
			double* UnifGrid = new double[nX];
			double hS = (X[1] - X[0]) / (nX - 1); // шаг сетки
			UnifGrid[0] = X[0];
			for (int j = 0; j < nX; ++j)
			{
				UnifGrid[j] = UnifGrid[0] + hS * j;
			}

			for (int i = 0; i < nX; ++i)
			{
				cout << "X[" << i << "]" << " = " << UnifGrid[i] << "  y_true = "<< Y[i] << "  Y[" << i << "] = " << DebugRes[3 * i] << endl;
			}
			cout << "Значение второй производной на левом конце отрезка:" << DebugRes[3 * 0 + 2] << endl;
			cout << "Значение второй производной на правом конце отрезка:" << DebugRes[3 * (nX - 1) + 2] << endl;
			cout << "///////////////////////Конец отладочной информации/////////////////////////" << endl << endl;;
			
		}
		else
		{
			status = dfdInterpolate1D(my_task, DF_INTERP, DF_METHOD_PP, nS, grid, DF_NON_UNIFORM_PARTITION, nDorder,
				dorder, NULL, splineValues, DF_NO_HINT, NULL);
		}
		if (status != DF_STATUS_OK)
		{
			throw "Error in dfdInterpolate1D";
		}

		status = dfDeleteTask(&my_task);
		if (status != DF_STATUS_OK)
		{
			throw "Error in dfDeleteTask";
		}
		
		//Расчет невязки
		for (int i = 0; i < nS; ++i)
		{
			F[i] = (y_true[i] - splineValues[i]) * (y_true[i] - splineValues[i]);
		}
		
	}
	catch (string ret)
	{
		delete[] scoeff;
		cout << ret << endl ;
	}
	
	delete[] scoeff;

}



struct SData                // Набор параметров, которые не изменяются в ходе итерации
{
	double* X;				// массив узлов сплайна (для построения)
	int nY;					// размерность векторной функции (для построения)
	double* grid;			// сетка (для расчета)
	double* splineValues;	// массив вычисленных значений сплайна на сетке grid
	double* y_true;         // Истинные значения функции на сетке grid
	bool DebugMode = false;
};


void fcn(				   //Обертка над CubicSpline для использования средств нелинейной оптимизации

	MKL_INT* nS,		   // число узлов сетки, на которой вычисляются значения сплайна (для расчета) nS
	MKL_INT* m,		       // число узлов сплайна == m (для построения) nX 
	double* Y,             // массив заданных значений векторной функции (для построения) (Этот вход и нужно сделать оптимальным)
	double* F,             //Расчет невязки (стремимся к ее минимуму) F
	void* user_data)       //Доп параметры, которые не изменяются в ходе итераций
{

	SData Params = *((SData*)user_data);
	CubicSpline(*m, Params.X, Params.nY, Y, *nS, Params.grid, Params.splineValues, Params.y_true, F, Params.DebugMode);

	
}



extern "C" _declspec(dllexport)
void OptimSplineInterpolation(
	int m,			 	  // число узлов сплайна на равномерной сетке == m (для построения)
	double* X, 			  // массив узлов сплайна на равномерной сетке (для построения)
	int nY,				  // размерность векторной функции (для построения)
	int nS,				  // число узлов сетки, на которой вычисляются значения сплайна (для расчета)
	double* grid,		  // Сетка, на которой происходит вычисление значений сплайна (для расчета)
	double* true_y,		  // Истинные значения функции на сетке grid 
	double* ApprStartVals,// Начальное приближение
	double* SplineValues, // Значения сплайна на сетке grid (искомое)
	int* StpReason,       // Причина остановки
	double* MinResVal,    // Минимальное значение невязки
	int MaxIters,         // Максимальное число итераций
	int *Iters,			  // Сделаное число итераций
	bool DebugMod = false)// Режим отладки	  
	
{
	
	SData Params;
	Params.X = X;
	Params.nY = nY;
	Params.grid = grid;
	Params.splineValues = SplineValues;
	Params.y_true = true_y;
	Params.DebugMode = DebugMod;

	int status;
	double eps[] = {
		1.0E-12,
		1.0E-12,
		1.0E-12,
		1.0E-12,
		1.0E-12,
		1.0E-12
	};

	double* Jacobian = new double[nS * m];
	double Jac_eps = 1.0E-8;

	MKL_INT taste_step_iters = 100;// максимальное число итераций при выборе пробного шага
	MKL_INT done_iter = 0;         // Число сделанных итераций

	double rs = 10;                // Начальное значение доверительного интервала

	// Возвращаемые значения
	double res_initial = 0;     // начальное значение невязки
	double res_final = 0;       // финальное значение невязки
	MKL_INT stop_criteria;      // причина остановки итераций
	MKL_INT check_data_info[4]; // результат проверки корректности данных

	ErrorEnum error = ErrorEnum(ErrorEnum::NO); // информация об ошибке

	_TRNSP_HANDLE_t handle = NULL;
	double* fvec = new double[nS];     // массив значений векторной функции
	double* fjac = new double[nS * m]; // массив с элементами матрицы Якоби
	
	try

	{	// Отладка (проверка того, что сплайн является интерполяционным, а также значения производных на концах отрезка равны 0)
		if (Params.DebugMode)
		{
			fcn(&m, &nS, ApprStartVals, fvec, static_cast<void*>(&Params));
			Params.DebugMode = false;
		}

		/////////
		// Инициализация задачи
		MKL_INT ret = dtrnlsp_init(&handle, &m, &nS, ApprStartVals, eps, &MaxIters, &taste_step_iters, &rs);
		if (ret != TR_SUCCESS)
		{
			throw (ErrorEnum(ErrorEnum::INIT_Err));
		}

		// Проверка корректности входных данных
		ret = dtrnlsp_check(&handle, &m, &nS, fjac, fvec, eps, check_data_info);
		if (ret != TR_SUCCESS)
		{
			throw (ErrorEnum(ErrorEnum::CHECK_Err));
		}

		MKL_INT RCI_Request = 0;
		
		
		while (true)
		{
			ret = dtrnlsp_solve(&handle, fvec, fjac, &RCI_Request);
			if (ret != TR_SUCCESS)
			{
				throw (ErrorEnum(ErrorEnum::SOLVE_Err));
			}
			if (RCI_Request == 0) continue;
			else if (RCI_Request == 1)
			{
				fcn(&nS, &m, ApprStartVals, fvec, static_cast<void*>(&Params));
			}
			else if (RCI_Request == 2)
			{
				ret = djacobix(fcn, &m, &nS, fjac, ApprStartVals, &Jac_eps, static_cast<void*>(&Params));
				if (ret != TR_SUCCESS)
				{
					throw (ErrorEnum(ErrorEnum::JACOBI_Err));
				}
			}
			else if (RCI_Request >= -6 && RCI_Request <= -1)
			{
				break;
			}
			else
			{
				throw (ErrorEnum(ErrorEnum::RCI_Err));
			}
		} 
		
		//if (IsThereAddonGrid) // Вычислить значения на дополнительной сетке
		//{
		//	double *tmpD;
		//	bool tmpB;
		//	tmpD = Params.grid;
		//	tmpB = Params.DebugMode;
		//	Params.grid = AddonGrid;
		//	Params.DebugMode = false;
		//	fcn(&AddonGridNodesNum, &m, ApprStartVals, fvec, static_cast<void*>(&Params)); // Вычислили значения на дополнительной сетке
		//	IsThereAddonGrid = false;
		//	Params.grid = tmpD;
		//	Params.DebugMode = tmpB;

		//}
		// 
		// Завершение итерационного процесса
		ret = dtrnlsp_get(&handle, &done_iter, &stop_criteria, &res_initial, &res_final);
		if (ret != TR_SUCCESS)
		{
			throw (ErrorEnum(ErrorEnum::GET_Err));
		}

		// Освобождение ресурсов
		ret = dtrnlsp_delete(&handle);
		if (ret != TR_SUCCESS)
		{
			throw (ErrorEnum(ErrorEnum::DELETE_Err));
		}
		
		
		*StpReason = stop_criteria; //Причина остановки
		*Iters = done_iter;			//Число сделаных итераций
		
		
	}
	catch (ErrorEnum _error) { error = _error; }
	// Освобождение памяти
	if (fvec != NULL) delete[] fvec;
	if (fjac != NULL) delete[] fjac;
	
}


extern "C" _declspec(dllexport)
void CalcSplineOnAddonGrid(
	MKL_INT * nS,		    // число узлов сетки, на которой вычисляются значения сплайна (для расчета) nS
	MKL_INT * m,		    // число узлов сплайна == m (для построения) nX 
	double* Y,              // массив заданных значений векторной функции (для построения) (Этот вход и нужно сделать оптимальным)
	double* F,              // Расчет невязки (стремимся к ее минимуму) F
	double* X,				// массив узлов сплайна (для построения)
	int nY,				    // размерность векторной функции (для построения)
	double* grid,			// сетка (для расчета)
	double* splineValues,	// массив вычисленных значений сплайна на сетке grid
	double* y_true,         // Истинные значения функции на сетке grid
	bool DebugMode = false)
{
	SData Params;
	Params.X = X;
	Params.nY = nY;
	Params.grid = grid;
	Params.splineValues = splineValues;
	Params.y_true = y_true;
	Params.DebugMode = DebugMode;
	CubicSpline(*m, Params.X, Params.nY, Y, *nS, Params.grid, Params.splineValues, Params.y_true, F, Params.DebugMode);
}