/**
 * @file listener.cpp
 *
 * @brief This class is for listener setting.
 *
 * Modified from the public domain source code in the following website. 
 * http://arsenmk.blogspot.com/2012/07/simple-implementation-of-observer.html
 *
 * @author Ryohei Yasuda
 * @date June 20th 2019
 * Copyright - Max Planck Florida Institute for Neuroscience 2019
 *
 */

#include "stdafx.h"
#include "Listener.h"
#include "DecodeEngine.h"

DLL_Listener** DLL_Listener::dllA = new DLL_Listener*[DecodeEngine::maxNinstances];

