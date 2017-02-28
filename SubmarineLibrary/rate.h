#pragma once
#include "stdafx.h"

namespace util
{
	class Rate
	{
		using time_unit = std::chrono::nanoseconds;
		using clock = std::chrono::high_resolution_clock;
		using time_point = std::chrono::time_point<clock>;

		time_unit desiredRateDuration;
		time_point loopStartTime;

	public:
		Rate(i32 hz)
		{
			double desiredRateInSeconds = (1.0 / static_cast<double>(hz));
			auto desiredRateDurationInSeconds = std::chrono::duration<double>(desiredRateInSeconds);
			this->desiredRateDuration = std::chrono::duration_cast<time_unit>(desiredRateDurationInSeconds);

			this->loopStartTime = clock::now();
		}

		void sleep()
		{
			time_point loopEndTime = clock::now();
			time_unit elapsedTime = loopEndTime - this->loopStartTime;
			
			if (elapsedTime < this->desiredRateDuration)
			{
				time_unit timeDifference = this->desiredRateDuration - elapsedTime;
				std::this_thread::sleep_for(timeDifference);
			}
			
			this->loopStartTime = clock::now();
		}
	};
}