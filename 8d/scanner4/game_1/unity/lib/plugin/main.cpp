#include <Eigen/Dense>
#include <Eigen/SVD>
#include <Eigen/Geometry>

/**
 * @param H input 3x3 matrix
 * @param R output 3x3 rotation matrix
 */
extern "C" __declspec(dllexport) void svd(double *H, double *R)
{
	Eigen::Map<Eigen::Matrix<double, 3, 3, Eigen::RowMajor>> h(H);
	Eigen::JacobiSVD<Eigen::Matrix3d> svd(h, Eigen::ComputeFullU | Eigen::ComputeFullV);
	Eigen::Matrix3d u = svd.matrixU();
	Eigen::Matrix3d v = svd.matrixV();
	double det = (u * v.transpose()).determinant();
	Eigen::Matrix3d correction = Eigen::Matrix3d::Identity();
	correction(2, 2) = det > 0.0 ? 1.0 : -1.0;
	Eigen::Matrix3d r = u * correction * v.transpose();
	auto rd = r.data();
	for (int i = 0; i < 9; i++)
		R[i] = rd[i];
}
